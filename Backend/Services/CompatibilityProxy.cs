using Backend.Models;

namespace Backend.Services;

/// <summary>
/// Реалізація патерна Proxy (Замісник).
/// Забезпечує шар кешування для важких операцій аналізу сумісності інгредієнтів.
/// </summary>
public class CompatibilityProxy : ICompatibilityStrategy
{
    /// <summary>
    /// Посилання на реальну стратегію перевірки (Real Subject).
    /// </summary>
    private readonly ICompatibilityStrategy _realStrategy;
    
    /// <summary>
    /// Внутрішнє сховище для кешування результатів. 
    /// </summary>
    private readonly Dictionary<string, CompatibilityResult> _cache = new();

    /// <summary>
    /// Ініціалізує новий екземпляр класу <see cref="CompatibilityProxy"/>.
    /// </summary>
    /// <param name="realStrategy">Реальна стратегія, яку потрібно загорнути в проксі.</param>
    public CompatibilityProxy(ICompatibilityStrategy realStrategy)
    {
        _realStrategy = realStrategy;
    }

    /// <summary>
    /// Перевіряє сумісність двох продуктів.
    /// Спочатку шукає результат у кеші, якщо не знайдено — викликає реальну стратегію.
    /// </summary>
    /// <param name="p1">Перший косметичний засіб.</param>
    /// <param name="p2">Другий косметичний засіб.</param>
    /// <returns>CompatibilityResult.</returns>
    public CompatibilityResult Check(Product p1, Product p2)
    {
        string key = $"{p1.Name}_{p2.Name}";

        if (_cache.ContainsKey(key))
        {
            return _cache[key];
        }

        CompatibilityResult result = _realStrategy.Check(p1, p2);
        _cache[key] = result;
        return result;
    }
}