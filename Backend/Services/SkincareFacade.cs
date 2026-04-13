using Backend.Models;
using Microsoft.Extensions.Localization;

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
        ILogger<DiagnosticDecorator> diagnosticLogger) 
    {
        _strategy = strategy;
        _logger = logger;
        _localizer = localizer;
        _diagnosticLogger = diagnosticLogger;
    }

    public string SimpleCheck(Product p1, Product p2)
    {
        _logger.LogInformation("Фасад аналізує сумісність...");
        
        var proxy = new CompatibilityProxy(_strategy);
        var decorated = new DiagnosticDecorator(proxy, _diagnosticLogger);

        bool isCompatible = decorated.Check(p1, p2);
        
        string resultKey = isCompatible ? "Compatible" : "Conflict";

        if (!isCompatible)
        {
            var notifier = new ConflictNotifier();
            notifier.Attach(new SecurityLogger(_logger));
            notifier.Notify($"Конфлікт: {p1.Name} + {p2.Name}");
        }
        
        var report = new SimpleTextReport(_localizer);
        return report.CreateFullReport(resultKey);
    }
    
    /// <summary>
    /// Отримує переклад за ключем із ресурсів.
    /// </summary>
    /// <param name="key">Ключ (наприклад, "NavConstructor")</param>
    /// <returns>Перекладений рядок</returns>
    public string GetTranslation(string key) => _localizer[key].Value;
}