using Backend.Models;

namespace Backend.Services;

/// <summary>
/// Патерн Builder для створення складних об'єктів Product.
/// </summary>
public class ProductBuilder
{
    private Product _product = new Product();

    public ProductBuilder Create(string name)
    {
        _product = new Product { Name = name };
        return this;
    }

    public ProductBuilder AddIngredient(string name, string type)
    {
        _product.Ingredients.Add(new Ingredient { Name = name, ActiveType = type });
        return this;
    }

    public Product Build() => _product;
}