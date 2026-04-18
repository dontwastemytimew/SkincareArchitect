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
    /// <c>true</c>, якщо продукти сумісні; 
    /// <c>false</c>, якщо знайдено критичний конфлікт (наприклад, Ретиноїди + Кислоти).
    /// </returns>
    public bool Check(Product p1, Product p2)
    {
        var settings = SystemSettings.GetInstance(new Microsoft.Extensions.Logging.Abstractions.NullLogger<SystemSettings>());

        foreach (var ing1 in p1.Ingredients)
        {
            foreach (var ing2 in p2.Ingredients)
            {
                if (ing1.ActiveType == "Acid" && ing2.ActiveType == "Acid")
                {
                    if (ing1.PHLevel < settings.CriticalPHThreshold && ing2.PHLevel < settings.CriticalPHThreshold)
                    {
                        _logger.LogWarning("Критично низький pH у парі: {P1} + {P2}", p1.Name, p2.Name);
                        return false;
                    }
                }
                
                if ((ing1.ActiveType == "Retinoid" || ing1.ActiveType == "Acid") && 
                    (ing2.ActiveType == "Retinoid" || ing2.ActiveType == "Acid"))
                {
                    double totalConc = ing1.Concentration + ing2.Concentration;
                    if (totalConc > settings.MaxSafeConcentration)
                    {
                        _logger.LogWarning("Агресивна концентрація: {Sum}%", totalConc);
                        return false;
                    }
                }
            }
        }
        return true;
    }
}