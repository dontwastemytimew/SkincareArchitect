using Backend.Models;

namespace Backend.Services;

/// <summary>
/// Реалізація конкретної стратегії перевірки сумісності.
/// Реалізує патерн <c>Strategy</c>.
/// </summary>
public class SimpleCompatibilityStrategy : ICompatibilityStrategy
{
    /// <summary>Сервіс логування для відстеження процесу перевірки.</summary>
    private readonly ILogger<SimpleCompatibilityStrategy> _logger;

    /// <summary>
    /// Ініціалізує новий екземпляр стратегії з підтримкою логування.
    /// </summary>
    /// <param name="logger">Екземпляр логера.</param>
    public SimpleCompatibilityStrategy(ILogger<SimpleCompatibilityStrategy> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Виконує алгоритм порівняння активних інгредієнтів двох продуктів.
    /// </summary>
    /// <param name="p1">Перший косметичний продукт.</param>
    /// <param name="p2">Другий косметичний продукт.</param>
    /// <returns>
    /// Об'єкт <see cref="CompatibilityResult"/>, що містить загальний статус сумісності 
    /// та список детальних попереджень для користувача.
    /// </returns>
    public CompatibilityResult Check(Product p1, Product p2)
    {
        var result = new CompatibilityResult();
        var settings =
            SystemSettings.GetInstance(new Microsoft.Extensions.Logging.Abstractions.NullLogger<SystemSettings>());

        foreach (var ing1 in p1.Ingredients)
        {
            foreach (var ing2 in p2.Ingredients)
            {
                CheckMissingConcentration(ing1, p1, result);
                CheckMissingConcentration(ing2, p2, result);

                // КОНФЛІКТ: Ретинол + Кислота
                bool isRetinoidAndAcid = (ing1.ActiveType == "Retinoid" && ing2.ActiveType == "Acid") ||
                                         (ing1.ActiveType == "Acid" && ing2.ActiveType == "Retinoid");
                if (isRetinoidAndAcid)
                {
                    result.IsSafe = false;
                    result.Warnings.Add($"Конфлікт бар'єру: знайдено '{p1.Name}' та '{p2.Name}'. Обидва компоненти агресивно оновлюють шкіру. Одночасне нанесення може викликати сильне подразнення. Рознесіть їх використання на різні дні або ранок/вечір.");
                }

                // КОНФЛІКТ: Вітамін С + Кислоти (Низький pH)
                bool isVitCAndAcid = (ing1.ActiveType == "Antioxidant" && ing1.Name.Contains("Vitamin C") &&
                                      ing2.ActiveType == "Acid") ||
                                     (ing2.ActiveType == "Antioxidant" && ing2.Name.Contains("Vitamin C") &&
                                      ing1.ActiveType == "Acid");
                if (isVitCAndAcid)
                {
                    result.Warnings.Add($"Ризик подразнення: '{p1.Name}' та '{p2.Name}' разом. Рекомендується використовувати Вітамін С вранці (з SPF), а кислоти — ввечері.");
                }

                // КОНФЛІКТ: Дві кислоти
                if (ing1.ActiveType == "Acid" && ing2.ActiveType == "Acid" && ing1.Name != ing2.Name)
                {
                    double totalConc = ing1.Concentration + ing2.Concentration;

                    if (totalConc > 0 && totalConc > settings.MaxSafeConcentration)
                    {
                        result.IsSafe = false;
                        result.Warnings.Add($"Хімічний опік: Сумарна концентрація кислот ('{p1.Name}' та '{p2.Name}') становить {totalConc}%. Це перевищує безпечний ліміт ({settings.MaxSafeConcentration}%).");
                    }
                    else if (totalConc == 0)
                    {
                        result.Warnings.Add(
                            $"Обережно: Ви використовуєте дві різні кислоти разом. Оскільки точна концентрація невідома, уважно слідкуйте за реакцією шкіри.");
                    }
                }
            }
        }
        
        result.Warnings = result.Warnings.Distinct().ToList();
        return result;
    }

    /// <summary>
    /// Допоміжний метод для перевірки відсутності точної концентрації активного інгредієнта.
    /// Якщо концентрація невідома (дорівнює 0) для агресивних активів (Кислоти або Ретиноїди), 
    /// метод формує безпечне попередження для користувача та додає його до загального результату, 
    /// автоматично уникаючи дублювання однакових повідомлень.
    /// </summary>
    /// <param name="ing">Інгредієнт, що зараз аналізується.</param>
    /// <param name="p">Продукт, якому належить даний інгредієнт.</param>
    /// <param name="result">Об'єкт результату сумісності, куди записується попередження.</param>
    private void CheckMissingConcentration(Ingredient ing, Product p, CompatibilityResult result)
    {
        if (ing.Concentration == 0 && (ing.ActiveType == "Acid" || ing.ActiveType == "Retinoid"))
        {
            string warning = $"Невідома концентрація '{ing.Name}'. Будь ласка, перевірте пакування засобу! Безпечний ліміт: для кислот (AHA/BHA) до 10%, для ретинолу до 1%. Якщо відсоток вищий, не поєднуйте з іншими активами.";
            
            if (!result.Warnings.Contains(warning))
            {
                result.Warnings.Add(warning);
            }
        }
    }
}