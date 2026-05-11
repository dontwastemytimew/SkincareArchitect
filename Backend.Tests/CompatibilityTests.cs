using Backend.Models;
using Backend.Services;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using Microsoft.Extensions.Localization;
using Moq;

namespace Backend.Tests;

[TestFixture]
public class CompatibilityTests
{
    private SimpleCompatibilityStrategy _strategy;
    private Mock<IStringLocalizer<SharedResource>> _mockLocalizer;

    [SetUp]
    public void Setup()
    {
        _mockLocalizer = new Mock<IStringLocalizer<SharedResource>>();
        _mockLocalizer.Setup(l => l[It.IsAny<string>()]).Returns((string key) => new LocalizedString(key, key));
        _mockLocalizer.Setup(l => l[It.IsAny<string>(), It.IsAny<object[]>()])
            .Returns((string key, object[] args) => new LocalizedString(key, key));

        _strategy = new SimpleCompatibilityStrategy(NullLogger<SimpleCompatibilityStrategy>.Instance, _mockLocalizer.Object);
    }

    [Test]
    public void Check_TwoStrongAcids_ReturnsFalse_DueToLowPH()
    {
        var p1 = new Product { Name = "Product A" };
        p1.Ingredients.Add(new Ingredient { Name = "Glycolic", ActiveType = "Acid", PHLevel = 3.0 });

        var p2 = new Product { Name = "Product B" };
        p2.Ingredients.Add(new Ingredient { Name = "Lactic", ActiveType = "Acid", PHLevel = 3.2 });
    
        var result = _strategy.Check(p1, p2);
    
        Assert.That(result.IsSafe, Is.False, "Дві кислоти з низьким pH мають бути небезпечними");
    }

    [Test]
    public void Check_HighConcentration_ReturnsFalse()
    {
        var p1 = new Product { Name = "Retinol" };
        p1.Ingredients.Add(new Ingredient { ActiveType = "Retinoid", Concentration = 2.0 });

        var p2 = new Product { Name = "Acid" };
        p2.Ingredients.Add(new Ingredient { ActiveType = "Acid", Concentration = 4.0 });
        
        var result = _strategy.Check(p1, p2);
        
        Assert.Multiple(() => {
            Assert.That(result.IsSafe, Is.False);
            Assert.That(result.Warnings, Is.Not.Empty, "Має бути хоча б одне попередження про концентрацію");
        });
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