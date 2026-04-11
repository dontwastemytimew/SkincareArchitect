using System.Collections;

namespace Backend.Models;

/// <summary>
/// Косметичний продукт, що містить інгредієнти.
/// </summary>
public class Product : ISkincareComponent
{
    public string Name { get; set; } = string.Empty;
    public List<Ingredient> Ingredients { get; set; } = new List<Ingredient>();

    public string GetName() => Name;
    public List<Ingredient> GetAllIngredients() => Ingredients;

    // Для ітератора: окремий продукт повертає сам себе
    public IEnumerator<ISkincareComponent> GetEnumerator() { yield return this; }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}