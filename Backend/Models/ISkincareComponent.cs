namespace Backend.Models;

/// <summary>
/// Патерн Composite. Спільний інтерфейс для окремих продуктів та їх наборів.
/// </summary>
public interface ISkincareComponent : IEnumerable<ISkincareComponent>
{
    string GetName();
    List<Ingredient> GetAllIngredients();
}