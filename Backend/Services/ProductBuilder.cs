using Backend.Models;

namespace Backend.Services;

/// <summary>
/// Реалізація патерна Builder для покрокового створення об'єктів косметичних засобів (Product).
/// </summary>
public class ProductBuilder
{
    /// <summary>
    /// Поточний екземпляр продукту, що будується.
    /// </summary>
    private Product _product = new Product();

    /// <summary>
    /// Ініціалізує новий процес побудови продукту з вказаною назвою.
    /// </summary>
    /// <param name="name">Назва косметичного засобу.</param>
    /// <returns>Поточний екземпляр ProductBuilder для ланцюжкового виклику.</returns>
    public ProductBuilder Create(string name)
    {
        _product = new Product { Name = name };
        return this;
    }

    /// <summary>
    /// Додає активний інгредієнт до продукту.
    /// </summary>
    /// <param name="name">Назва інгредієнта (наприклад, "Retinol").</param>
    /// <param name="type">Тип активної речовини (наприклад, "Retinoid").</param>
    /// <returns>Поточний екземпляр ProductBuilder.</returns>
    public ProductBuilder AddIngredient(string name, string type)
    {
        _product.Ingredients.Add(new Ingredient { Name = name, ActiveType = type });
        return this;
    }

    /// <summary>
    /// Повертає повністю сформований об'єкт Product.
    /// </summary>
    /// <returns>Сконструйований екземпляр класу Product.</returns>
    public Product Build() => _product;
}