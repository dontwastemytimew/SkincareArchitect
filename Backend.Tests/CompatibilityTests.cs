using Backend.Models;
using Backend.Services;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;

namespace Backend.Tests;

[TestFixture]
public class CompatibilityTests
{
    private SimpleCompatibilityStrategy _strategy;

    [SetUp]
    public void Setup()
    {
        _strategy = new SimpleCompatibilityStrategy(NullLogger<SimpleCompatibilityStrategy>.Instance);
    }

    [Test]
    public void Check_RetinoidAndAcid_ReturnsFalse()
    {
        var p1 = new Product { Name = "Retinol" };
        p1.Ingredients.Add(new Ingredient { Name = "Pure Retinol", ActiveType = "Retinoid" });

        var p2 = new Product { Name = "AHA Acid" };
        p2.Ingredients.Add(new Ingredient { Name = "Glycolic Acid", ActiveType = "Acid" });
        
        var result = _strategy.Check(p1, p2);
        
        Assert.That(result, Is.False, "Ретиноїди та кислоти мають бути несумісними");
    }
    
    [Test]
    public void Builder_BuildsProductCorrectly()
    {
        var builder = new ProductBuilder();
        var product = builder.Create("Test Serum")
            .AddIngredient("BHA", "Acid")
            .Build();

        Assert.Multiple(() =>
        {
            Assert.That(product.Name, Is.EqualTo("Test Serum"));
            Assert.That(product.Ingredients[0].ActiveType, Is.EqualTo("Acid"));
        });
    }

    [Test]
    public void Check_CompatibleProducts_ReturnsTrue()
    {
        var p1 = new Product { Name = "Moisturizer" };
        p1.Ingredients.Add(new Ingredient { Name = "Glycerin", ActiveType = "Moisturizer" });

        var p2 = new Product { Name = "Centella" };
        p2.Ingredients.Add(new Ingredient { Name = "Centella", ActiveType = "Soothing" });

        var result = _strategy.Check(p1, p2);
        Assert.That(result, Is.True);
    }
    
    [Test]
    public void Check_Compatibility_IsSymmetric()
    {
        var p1 = new Product { Name = "Retinol" };
        p1.Ingredients.Add(new Ingredient { ActiveType = "Retinoid" });
        var p2 = new Product { Name = "Acid" };
        p2.Ingredients.Add(new Ingredient { ActiveType = "Acid" });

        var res1 = _strategy.Check(p1, p2);
        var res2 = _strategy.Check(p2, p1);

        Assert.That(res1, Is.EqualTo(res2), "Результат має бути однаковим незалежно від порядку");
    }
}