using Backend.Models;

namespace Backend.Services;

/// <summary>
/// Інтерфейс стратегії для перевірки сумісності продуктів.
/// Патерн: Strategy.
/// </summary>
public interface ICompatibilityStrategy
{
    /// <summary> Перевіряє сумісність двох продуктів. </summary>
    bool Check(Product p1, Product p2);
}