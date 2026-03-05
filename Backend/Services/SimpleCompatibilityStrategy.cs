using Backend.Models;
using Microsoft.Extensions.Logging;

namespace Backend.Services;

/// <summary>
/// Реалізація базової стратегії перевірки сумісності інгредієнтів.
/// Патерн: Strategy.
/// </summary>
public class SimpleCompatibilityStrategy : ICompatibilityStrategy
{
    private readonly ILogger<SimpleCompatibilityStrategy> _logger;

    public SimpleCompatibilityStrategy(ILogger<SimpleCompatibilityStrategy> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Перевіряє, чи немає конфлікту між активними компонентами.
    /// </summary>
    public bool Check(Product p1, Product p2)
    {
        _logger.LogInformation("Аналіз сумісності: {P1} та {P2}", p1.Name, p2.Name);

        foreach (var ing1 in p1.Ingredients)
        {
            foreach (var ing2 in p2.Ingredients)
            {
                if ((ing1.ActiveType == "Retinoid" && ing2.ActiveType == "Acid") ||
                    (ing1.ActiveType == "Acid" && ing2.ActiveType == "Retinoid"))
                {
                    _logger.LogWarning("Знайдено конфлікт: {I1} та {I2}!", ing1.Name, ing2.Name);
                    return false;
                }
            }
        }
        return true;
    }
}