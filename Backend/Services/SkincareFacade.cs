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
    /// Виконує комплексну перевірку сумісності компонентів догляду.
    /// </summary>
    /// <param name="component">Компонент догляду (окремий продукт або ціла рутина - Composite).</param>
    /// <returns>Локалізований текстовий звіт.</returns>
    public string SimpleCheck(ISkincareComponent routine)
    {
        // Отримуємо всі інгредієнти одним списком завдяки Composite!
        var allIngredients = routine.GetAllIngredients();
    
        var proxy = new CompatibilityProxy(_strategy);
        var decorated = new DiagnosticDecorator(proxy, _diagnosticLogger);

        bool isCompatible = true;
        for (int i = 0; i < allIngredients.Count; i++)
        {
            for (int j = i + 1; j < allIngredients.Count; j++)
            {
                var tempP1 = new Product { Name = "Active 1", Ingredients = new List<Ingredient> { allIngredients[i] } };
                var tempP2 = new Product { Name = "Active 2", Ingredients = new List<Ingredient> { allIngredients[j] } };

                if (!decorated.Check(tempP1, tempP2)) 
                { 
                    isCompatible = false; 
                    break; 
                }
            }
            if (!isCompatible) break;
        }

        string resultKey = isCompatible ? "Compatible" : "Conflict";

        if (!isCompatible)
        {
            var notifier = new ConflictNotifier();
            notifier.Attach(new SecurityLogger(_logger));
            notifier.Notify($"Виявлено несумісність у наборі: {routine.GetName()}");
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