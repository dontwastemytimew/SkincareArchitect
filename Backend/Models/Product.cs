using System.Collections;

namespace Backend.Models;

/// <summary>
/// Представляє окремий косметичний продукт.
/// Є "листком" у структурі патерна Composite.
/// </summary>
public class Product : ISkincareComponent
{
    /// <summary> Назва косметичного засобу. </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary> Список інгредієнтів, що входять до складу продукту. </summary>
    public List<Ingredient> Ingredients { get; set; } = new List<Ingredient>();

    /// <inheritdoc/>
    public string GetName() => Name;
    
    /// <inheritdoc/>
    public List<Ingredient> GetAllIngredients() => Ingredients;

    /// <summary>
    /// Реалізація патерна Iterator.
    /// Повертає перелічувач, який містить лише цей продукт.
    /// </summary>
    public IEnumerator<ISkincareComponent> GetEnumerator() { yield return this; }
    
    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}