using Backend.Models;

namespace Backend.Services;

/// <summary>
/// Інтерфейс, що визначає контракт для алгоритмів перевірки сумісності.
/// Реалізує патерн <c>Strategy</c>.
/// </summary>
public interface ICompatibilityStrategy
{
    /// <summary>
    /// Виконує перевірку сумісності між двома об'єктами продуктів.
    /// </summary>
    /// <param name="p1">Перший продукт для порівняння.</param>
    /// <param name="p2">Другий продукт для порівняння.</param>
    /// <returns>Об'єкт CompatibilityResult, що містить загальний статус безпеки та детальні попередження.</returns>
    CompatibilityResult Check(Product p1, Product p2);
}