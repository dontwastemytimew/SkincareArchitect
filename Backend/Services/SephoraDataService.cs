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
        // Parallel.ForEach автоматично розбиває список на частини і віддає різним ядрам
        Parallel.ForEach(products, product =>
        {
            ProcessSingleProduct(product);
        });
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
        
        string[] rawIngredients = product.Ingredients.Split(new[] { ',', '.', ';' }, StringSplitOptions.RemoveEmptyEntries);

        foreach (var raw in rawIngredients)
        {
            string cleanName = raw.Trim().ToLower();
            
            if (Regex.IsMatch(cleanName, @"\bretin[oa]l\b|\bretinoid\b"))
            {
                product.ParsedIngredients.Add(new Ingredient { 
                    Name = raw.Trim(), 
                    ActiveType = "Retinoid", 
                    Concentration = ExtractConcentration(cleanName) 
                });
            }
            else if (Regex.IsMatch(cleanName, @"\bacid\b"))
            {
                product.ParsedIngredients.Add(new Ingredient { 
                    Name = raw.Trim(), 
                    ActiveType = "Acid", 
                    PHLevel = 3.2 
                });
            }
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
}