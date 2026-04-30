namespace Backend.Models;

/// <summary>
/// Об'єкт, що зберігає результати перевірки сумісності продуктів.
/// </summary>
public class CompatibilityResult
{
    /// <summary>Загальний статус безпеки (true - безпечно, false - є критичні конфлікти).</summary>
    public bool IsSafe { get; set; } = true;
    
    /// <summary>Список попереджень та знайдених проблем для користувача.</summary>
    public List<string> Warnings { get; set; } = new();
}