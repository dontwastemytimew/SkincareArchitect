namespace Backend.Services;

/// <summary>
/// Базовий клас для генерації звітів.
/// Патерн Template Method.
/// </summary>
public abstract class ReportGenerator
{
    // Шаблонний метод
    public string CreateFullReport(string content)
    {
        return $"{FormatHeader()}\n{FormatBody(content)}\n{FormatFooter()}";
    }

    protected abstract string FormatHeader();
    protected abstract string FormatBody(string content);
    private string FormatFooter() => "\n[SkincareArchitect 2026]";
}

public class SimpleTextReport : ReportGenerator
{
    protected override string FormatHeader() => "--- РЕЗУЛЬТАТ АНАЛІЗУ ---";
    protected override string FormatBody(string content) => $"Повідомлення: {content}";
}