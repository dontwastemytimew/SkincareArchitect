using Microsoft.Extensions.Localization;
namespace Backend.Services;

/// <summary>
/// Базовий клас для генерації звітів.
/// Патерн Template Method.
/// </summary>
public abstract class ReportGenerator
{
    protected readonly IStringLocalizer _localizer;

    protected ReportGenerator(IStringLocalizer localizer)
    {
        _localizer = localizer;
    }

    public string CreateFullReport(string contentKey)
    {
        return $"{FormatHeader()}\n{FormatBody(contentKey)}\n{FormatFooter()}";
    }

    protected abstract string FormatHeader();
    protected abstract string FormatBody(string contentKey);
    protected abstract string FormatFooter();
}

public class SimpleTextReport : ReportGenerator
{
    public SimpleTextReport(IStringLocalizer localizer) : base(localizer) { }
    
    protected override string FormatHeader() => _localizer["RoutineHeader"];
    
    protected override string FormatBody(string contentKey) => _localizer[contentKey];
        
    protected override string FormatFooter() => _localizer["RoutineFooter"];
}