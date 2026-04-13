using Microsoft.Extensions.Localization;
namespace Backend.Services;

/// <summary>
/// Абстрактний базовий клас, що реалізує патерн Template Method для генерації звітів.
/// Визначає скелет алгоритму формування звіту.
/// </summary>
public abstract class ReportGenerator
{
    /// <summary>
    /// Локалізатор для підтримки багатомовності у звітах.
    /// </summary>
    protected readonly IStringLocalizer _localizer;

    /// <summary>
    /// Конструктор класу ReportGenerator.
    /// </summary>
    /// <param name="localizer">Сервіс локалізації рядків.</param>
    protected ReportGenerator(IStringLocalizer localizer)
    {
        _localizer = localizer;
    }

    /// <summary>
    /// Основний Template Method, який визначає послідовність кроків формування повного звіту.
    /// </summary>
    /// <param name="contentKey">Ключ локалізації для основного тексту звіту.</param>
    /// <returns>Готовий текстовий звіт.</returns>
    public string CreateFullReport(string contentKey)
    {
        return $"{FormatHeader()}\n{FormatBody(contentKey)}\n{FormatFooter()}";
    }

    /// <summary>
    /// Абстрактний метод для формування заголовка звіту. Реалізується в конкретних підкласах.
    /// </summary>
    protected abstract string FormatHeader();
    
    /// <summary>
    /// Абстрактний метод для формування тіла звіту на основі ключа вмісту.
    /// </summary>
    protected abstract string FormatBody(string contentKey);
    
    /// <summary>
    /// Абстрактний метод для формування завершальної частини (футера) звіту.
    /// </summary>
    protected abstract string FormatFooter();
}

/// <summary>
/// Конкретна реалізація генератора звітів у текстовому форматі.
/// </summary>
public class SimpleTextReport : ReportGenerator
{
    /// <summary>
    /// Конструктор SimpleTextReport.
    /// </summary>
    /// <param name="localizer">Сервіс локалізації рядків.</param>
    public SimpleTextReport(IStringLocalizer localizer) : base(localizer) { }
    
    /// <summary>
    /// Повертає локалізований заголовок звіту.
    /// </summary>
    protected override string FormatHeader() => _localizer["RoutineHeader"];
    
    /// <summary>
    /// Повертає локалізований основний вміст звіту за вказаним ключем.
    /// </summary>
    protected override string FormatBody(string contentKey) => _localizer[contentKey];
    
    /// <summary>
    /// Повертає локалізований футер звіту.
    /// </summary>
    protected override string FormatFooter() => _localizer["RoutineFooter"];
}