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
        
        Assert.That(result, Is.False, "Ретиноїди та кислоти мають бути несумісними!");
    }
}