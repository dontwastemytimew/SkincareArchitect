using Microsoft.Extensions.Localization;
using Backend.Models;
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
    /// <param name="analysisResult">Об'єкт результату аналізу, що містить статус та попередження.</param>
    /// <returns>Готовий текстовий звіт.</returns>
    public string CreateFullReport(CompatibilityResult analysisResult)
    {
        return $"{FormatHeader()}\n\n{FormatBody(analysisResult)}\n\n{FormatFooter()}";
    }

    /// <summary>
    /// Абстрактний метод для формування заголовка звіту. Реалізується в конкретних підкласах.
    /// </summary>
    protected abstract string FormatHeader();
    
    /// <summary>
    /// Абстрактний метод для формування тіла звіту.
    /// </summary>
    protected abstract string FormatBody(CompatibilityResult analysisResult);
    
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
    /// Повертає локалізований основний вміст звіту.
    /// </summary>
    protected override string FormatBody(CompatibilityResult analysisResult)
    {
        if (analysisResult.IsSafe && analysisResult.Warnings.Count == 0)
        {
            return _localizer["AllGood"] ?? "Ваша рутина ідеальна! Конфліктів не знайдено.";
        }

        string body = "";
        
        if (!analysisResult.IsSafe)
        {
            body += "ЗНАЙДЕНО КРИТИЧНІ КОНФЛІКТИ:\n";
        }
        else
        {
            body += "РЕКОМЕНДАЦІЇ ТА ПОПЕРЕДЖЕННЯ:\n";
        }

        foreach (var warning in analysisResult.Warnings)
        {
            body += $"- {warning}\n";
        }

        return body;
    }
    
    /// <summary>
    /// Повертає локалізований футер звіту.
    /// </summary>
    protected override string FormatFooter() => _localizer["RoutineFooter"];
}