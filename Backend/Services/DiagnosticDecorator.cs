using Backend.Models;
using System.Diagnostics;

namespace Backend.Services;

/// <summary>
/// Реалізація патерна Decorator для моніторингу продуктивності.
/// Додає функціонал вимірювання часу виконання до будь-якої стратегії сумісності.
/// </summary>
public class DiagnosticDecorator : ICompatibilityStrategy
{
    /// <summary>
    /// Оригінальна стратегія, яка обгортається декоратором.
    /// </summary>
    private readonly ICompatibilityStrategy _innerStrategy;
    
    /// <summary>
    /// Логер для виводу результатів діагностики в консоль або файл.
    /// </summary>
    private readonly ILogger<DiagnosticDecorator> _logger;

    /// <summary>
    /// Ініціалізує новий екземпляр класу <see cref="DiagnosticDecorator"/>.
    /// </summary>
    /// <param name="innerStrategy">Об'єкт стратегії, який потрібно задекорувати.</param>
    /// <param name="logger">Сервіс логування.</param>
    public DiagnosticDecorator(ICompatibilityStrategy innerStrategy, ILogger<DiagnosticDecorator> logger)
    {
        _innerStrategy = innerStrategy;
        _logger = logger;
    }

    /// <summary>
    /// Перевіряє сумісність продуктів, вимірюючи швидкість роботи вкладеної стратегії.
    /// </summary>
    /// <param name="p1">Перший косметичний засіб.</param>
    /// <param name="p2">Другий косметичний засіб.</param>
    /// <returns>CompatibilityResult.</returns>
    public CompatibilityResult Check(Product p1, Product p2)
    {
        var sw = Stopwatch.StartNew();
        
        CompatibilityResult result = _innerStrategy.Check(p1, p2);
        
        sw.Stop();
        _logger.LogInformation("Діагностика: час перевірки склав {MS} мс", sw.Elapsed.TotalMilliseconds);
        
        return result;
    }
}