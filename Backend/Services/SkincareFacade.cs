using Backend.Models;
using Microsoft.Extensions.Localization;
using Backend.Resources;
using Backend.Services;

namespace Backend.Services;

/// <summary>
/// Патерн Facade для керування складними операціями аналізу догляду.
/// </summary>
public class SkincareFacade
{
    private readonly ICompatibilityStrategy _strategy;
    private readonly ILogger<SkincareFacade> _logger;
    private readonly IStringLocalizer<SharedResource> _localizer;
    private readonly ILogger<DiagnosticDecorator> _diagnosticLogger;

    public SkincareFacade(
        ICompatibilityStrategy strategy, 
        ILogger<SkincareFacade> logger, 
        IStringLocalizer<SharedResource> localizer,
        ILogger<DiagnosticDecorator> diagnosticLogger) // DI передасть його сюди
    {
        _strategy = strategy;
        _logger = logger;
        _localizer = localizer;
        _diagnosticLogger = diagnosticLogger;
    }

    public string SimpleCheck(Product p1, Product p2)
    {
        var proxy = new CompatibilityProxy(_strategy);
        var decorated = new DiagnosticDecorator(proxy, _diagnosticLogger);
        
        bool isCompatible = decorated.Check(p1, p2);
        
        if (!isCompatible)
        {
            var notifier = new ConflictNotifier();
            notifier.Attach(new SecurityLogger(_logger));
            notifier.Notify($"Увага! Конфлікт між {p1.Name} та {p2.Name}");
        }
        
        var report = new SimpleTextReport();
        return report.CreateFullReport(isCompatible ? _localizer["Greeting"] : "Conflict detected!");
    }
}