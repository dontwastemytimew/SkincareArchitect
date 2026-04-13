namespace Backend.Models;

/// <summary>
/// Інтерфейс компонента догляду за шкірою.
/// Реалізує патерн Composite для об'єднання окремих продуктів у набори (рутини).
/// </summary>
public interface ISkincareComponent : IEnumerable<ISkincareComponent>
{
    /// <summary> Повертає назву компонента або назву всієї рутини. </summary>
    /// <returns>Рядок з назвою.</returns>
    string GetName();
    
    /// <summary> Повертає повний список інгредієнтів усіх вкладених засобів. </summary>
    /// <returns>Список об'єктів Ingredient.</returns>
    List<Ingredient> GetAllIngredients();
}