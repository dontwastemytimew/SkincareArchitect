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
    public void Check_TwoStrongAcids_ReturnsFalse_DueToLowPH()
    {
        var p1 = new Product { Name = "Acid 1" };
        p1.Ingredients.Add(new Ingredient { ActiveType = "Acid", PHLevel = 3.0 });

        var p2 = new Product { Name = "Acid 2" };
        p2.Ingredients.Add(new Ingredient { ActiveType = "Acid", PHLevel = 3.2 });
        
        var result = _strategy.Check(p1, p2);
        
        Assert.That(result, Is.False, "Дві кислоти з pH < 3.5 мають бути несумісними");
    }

    [Test]
    public void Check_HighConcentration_ReturnsFalse()
    {
        var p1 = new Product { Name = "Retinol" };
        p1.Ingredients.Add(new Ingredient { ActiveType = "Retinoid", Concentration = 2.0 });

        var p2 = new Product { Name = "Acid" };
        p2.Ingredients.Add(new Ingredient { ActiveType = "Acid", Concentration = 4.0 });
        
        var result = _strategy.Check(p1, p2);
        
        Assert.That(result, Is.False, "Сумарна концентрація 6% > 5% має бути небезпечною");
    }

    [Test]
    public void Builder_BuildsProductWithChemicalProperties()
    {
        var builder = new ProductBuilder();
        var product = builder.Create("Safe Serum")
            .AddIngredient("Vitamin C", "Acid", 3.8, 2.0)
            .Build();

        Assert.Multiple(() =>
        {
            Assert.That(product.Ingredients[0].PHLevel, Is.EqualTo(3.8));
            Assert.That(product.Ingredients[0].Concentration, Is.EqualTo(2.0));
        });
    }
}