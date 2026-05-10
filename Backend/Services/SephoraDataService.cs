using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using Backend.Models;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Backend.Services;

/// <summary>
/// Сервіс для обробки великих обсягів даних.
/// Містить реалізацію як послідовних, так і мультипоточних алгоритмів.
/// </summary>
public class SephoraDataService
{
    private readonly string _filePath;

    /// <summary>
    /// Ініціалізує сервіс роботи з даними.
    /// </summary>
    /// <param name="filePath">Шлях до локального датасету.</param>
    public SephoraDataService(string filePath)
    {
        _filePath = filePath;
    }

    /// <summary>
    /// Зчитує тисячі записів про продукти з CSV файлу в пам'ять.
    /// Демонструє завантаження даних із зовнішнього джерела.
    /// </summary>
    /// <returns>Список об'єктів SephoraProduct.</returns>
    public List<SephoraProduct> LoadProducts()
    {
        using var reader = new StreamReader(_filePath);
        using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            HeaderValidated = null,
            MissingFieldFound = null
        });

        return csv.GetRecords<SephoraProduct>().ToList();
    }

    /// <summary>
    /// Послідовна обробка інгредієнтів (1 потік).
    /// Використовується як еталон для вимірювання прискорення.
    /// </summary>
    /// <param name="products">Список продуктів для аналізу.</param>
    /// <returns>Час виконання алгоритму в мілісекундах.</returns>
    public long ParseSequential(List<SephoraProduct> products)
    {
        var sw = Stopwatch.StartNew();
        foreach (var product in products)
        {
            ProcessSingleProduct(product);
        }

        sw.Stop();
        return sw.ElapsedMilliseconds;
    }

    /// <summary>
    /// Мультипоточна обробка інгредієнтів.
    /// Використовує TPL (Task Parallel Library) для розподілу обчислень між усіма доступними ядрами ЦП.
    /// </summary>
    /// <param name="products">Список продуктів для аналізу.</param>
    /// <returns>Час виконання паралельного алгоритму в мілісекундах.</returns>
    public long ParseParallel(List<SephoraProduct> products)
    {
        var sw = Stopwatch.StartNew();
        Parallel.ForEach(products, product => { ProcessSingleProduct(product); });
        sw.Stop();
        return sw.ElapsedMilliseconds;
    }

    /// <summary>
    /// Складна обчислювальна логіка парсингу одного продукту.
    /// Використовує регулярні вирази для лексичного аналізу тексту.
    /// </summary>
    /// <param name="product">Продукт, чиї інгредієнти потрібно розібрати.</param>
    private void ProcessSingleProduct(SephoraProduct product)
    {
        if (string.IsNullOrEmpty(product.Ingredients)) return;

        string[] rawIngredients =
            product.Ingredients.Split(new[] { ',', '.', ';' }, StringSplitOptions.RemoveEmptyEntries);

        foreach (var raw in rawIngredients)
        {
            string cleanName = raw.Trim().ToLower();

            // Пошук активних ретиноїдів
            if (Regex.IsMatch(cleanName, @"\bretin[oa]l\b|\bretinoid\b|\badapalene\b"))
            {
                double conc = ExtractConcentration(cleanName);
                if (conc == 0) conc = ExtractConcentration(product.ProductName.ToLower());

                product.ParsedIngredients.Add(new Ingredient
                {
                    Name = raw.Trim(),
                    ActiveType = "Retinoid",
                    Concentration = conc
                });
            }
            // Пошук активних кислот
            else if (Regex.IsMatch(cleanName, @"\b(glycolic|salicylic|lactic|mandelic|tartaric)\b.*\bacid\b"))
            {
                double conc = ExtractConcentration(cleanName);
                if (conc == 0) conc = ExtractConcentration(product.ProductName.ToLower());

                product.ParsedIngredients.Add(new Ingredient
                {
                    Name = raw.Trim(),
                    ActiveType = "Acid",
                    PHLevel = 3.2,
                    Concentration = conc
                });
            }
        }

        // Визначення рекомендованого часу
        if (product.ParsedIngredients.Any(i => i.ActiveType == "Retinoid") ||
            product.ProductName.Contains("Night", StringComparison.OrdinalIgnoreCase))
        {
            product.PreferredTime = "evening";
        }
        else if (product.ProductName.Contains("SPF", StringComparison.OrdinalIgnoreCase) ||
                 product.ProductName.Contains("Sunscreen", StringComparison.OrdinalIgnoreCase) ||
                 product.ProductName.Contains("Day", StringComparison.OrdinalIgnoreCase))
        {
            product.PreferredTime = "morning";
        }
        else
        {
            product.PreferredTime = "both";
        }

        // Визначення черговості нанесення за текстурою
        string lowerName = product.ProductName.ToLower();

        if (lowerName.Contains("toner") || lowerName.Contains("essence") || lowerName.Contains("liquid"))
        {
            product.ApplicationOrder = 1; // Найрідші (тоніки)
        }
        else if (lowerName.Contains("serum") || lowerName.Contains("ampoule") || lowerName.Contains("gel"))
        {
            product.ApplicationOrder = 2; // Сироватки
        }
        else if (lowerName.Contains("cream") || lowerName.Contains("moisturizer") || lowerName.Contains("lotion"))
        {
            product.ApplicationOrder = 3; // Креми
        }
        else if (lowerName.Contains("spf") || lowerName.Contains("sunscreen"))
        {
            product.ApplicationOrder = 4; // Заключний шар
        }
        else
        {
            product.ApplicationOrder = 3; // За замовчуванням вважаємо кремовою текстурою
        }
    }

    /// <summary>
    /// Допоміжний метод для витягування відсотка концентрації з тексту.
    /// </summary>
    /// <param name="text">Текст, що містить відсоток (наприклад, "retinol 0.5%").</param>
    /// <returns>Числове значення концентрації або 0.0.</returns>
    private double ExtractConcentration(string text)
    {
        var match = Regex.Match(text, @"(\d+(\.\d+)?)%");
        if (match.Success && double.TryParse(match.Groups[1].Value, out double val))
            return val;
        return 0.0;
    }

    /// <summary>
    /// Перевіряє, чи належить продукт до догляду за обличчям.
    /// </summary>
    public bool IsValidSkincare(SephoraProduct product)
    {
        if (string.IsNullOrEmpty(product.ProductName)) return false;
        
        string searchArea = $"{product.ProductName} {product.BrandName}".ToLower();

        var stopWords = new[]
        {
            "hair", "shampoo", "conditioner", "scalp", "mousse", "styler", "waver", "curl", "brush", "comb", "dryer",
            "iron", "pomade", "haircare", "shave", "aftershave", "beard", "lip", "anastasia beverly hills",
            "makeup", "foundation", "concealer", "lipstick", "mascara", "eyeshadow", "palette", "blush", "gloss",
            "highlighter", "bronzer", "contour", "liner", "sharpener", "lashes", "brow", "pencil", "primer",
            "setting spray", "powder", "beautyblender", "berdoues", "bio ionic", "bumble and bumble", "burberry",
            "body", "hand", "foot", "nail", "butter", "deodorant", "bath", "salt", "soap", "scrub", "tan", "sunless",
            "perfume", "fragrance", "cologne", "parfum", "spray", "mist", "diffuser", "candle", "edp", "edt",
            "rollerball", "calvin klein", "caliray", "chanel", "corrector", "filter", "chloé", "color",
            "scissors", "tweezers", "roller", "set", "kit", "sponge", "applicator", "bag", "case", "mirror",
            "aerin", "armani", "tangier", "conditioning", "gift", "value", "acqua di parma", "commodity"
        };

        foreach (var word in stopWords)
        {
            if (searchArea.Contains(word)) return false;
        }

        return true;
    }
}