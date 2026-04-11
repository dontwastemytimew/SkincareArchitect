using Backend.Models;
using System.Diagnostics;

namespace Backend.Services;

/// <summary>
/// Патерн Decorator для вимірювання часу виконання стратегії.
/// </summary>
public class DiagnosticDecorator : ICompatibilityStrategy
{
    private readonly ICompatibilityStrategy _innerStrategy;
    private readonly ILogger<DiagnosticDecorator> _logger;

    public DiagnosticDecorator(ICompatibilityStrategy innerStrategy, ILogger<DiagnosticDecorator> logger)
    {
        _innerStrategy = innerStrategy;
        _logger = logger;
    }

    public bool Check(Product p1, Product p2)
    {
        var sw = Stopwatch.StartNew();
        
        bool result = _innerStrategy.Check(p1, p2);
        
        sw.Stop();
        _logger.LogInformation("Діагностика: час перевірки склав {MS} мс", sw.Elapsed.TotalMilliseconds);
        
        return result;
    }
}