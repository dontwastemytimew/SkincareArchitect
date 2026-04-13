using Backend.Models;
using Microsoft.Extensions.Localization;

namespace Backend.Services;

/// <summary>
/// Надає спрощений інтерфейс для взаємодії зі складною системою аналізу складу.
/// Використовує патерн <c>Facade</c>.
/// </summary>
public class SkincareFacade
{
    private readonly ICompatibilityStrategy _strategy;
    private readonly ILogger<SkincareFacade> _logger;
    private readonly IStringLocalizer<SharedResource> _localizer;
    private readonly ILogger<DiagnosticDecorator> _diagnosticLogger;

    /// <summary>
    /// Конструктор фасаду, що впроваджує необхідні залежності через DI.
    /// </summary>
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

    /// <summary>
    /// Виконує комплексну перевірку двох засобів, включаючи логування, проксі-захист та генерацію звіту.
    /// </summary>
    /// <remarks>
    /// Метод задіює патерни: Proxy, Decorator, Observer та Template Method.
    /// </remarks>
    /// <param name="p1">Перший продукт для аналізу.</param>
    /// <param name="p2">Другий продукт для аналізу.</param>
    /// <returns>Локалізований текстовий звіт про сумісність.</returns>
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
    /// Отримує локалізований рядок за заданим ключем.
    /// Використовується фронтендом для динамічної зміни мови інтерфейсу.
    /// </summary>
    /// <param name="key">Ключ ресурсу (наприклад, "BtnRunScan").</param>
    /// <returns>Значення перекладу з поточного .resx файлу.</returns>
    public string GetTranslation(string key) => _localizer[key].Value;
}