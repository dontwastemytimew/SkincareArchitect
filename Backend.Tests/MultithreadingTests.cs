using Backend.Models;
using Backend.Services;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace Backend.Tests;

[TestFixture]
public class MultithreadingTests
{
    private SephoraDataService _service;

    [SetUp]
    public void Setup()
    {
        _service = new SephoraDataService("Data/product_info.csv");
    }
    
    [Test]
    public void ParallelParsing_YieldsSameResults_AsSequential()
    {
        var rawProducts = new List<SephoraProduct>();
        for (int i = 0; i < 1000; i++)
        {
            rawProducts.Add(new SephoraProduct { 
                ProductName = $"Product {i}", 
                Ingredients = "Water, Retinol 0.5%, Salicylic Acid 2%" 
            });
        }
        
        var listForSeq = rawProducts.Select(p => new SephoraProduct { ProductName = p.ProductName, Ingredients = p.Ingredients }).ToList();
        var listForPar = rawProducts.Select(p => new SephoraProduct { ProductName = p.ProductName, Ingredients = p.Ingredients }).ToList();

        _service.ParseSequential(listForSeq);
        _service.ParseParallel(listForPar);
        
        Assert.Multiple(() =>
        {
            Assert.That(listForPar.Count, Is.EqualTo(listForSeq.Count));
            int totalSeq = listForSeq.Sum(p => p.ParsedIngredients.Count);
            int totalPar = listForPar.Sum(p => p.ParsedIngredients.Count);
            Assert.That(totalPar, Is.EqualTo(totalSeq), "Мультипоточність не має втрачати дані");
        });
    }
    
    [Test]
    public void Parsing_ShouldHandleEmptyOrInvalidIngredients()
    {
        var edgeCases = new List<SephoraProduct>
        {
            new SephoraProduct { ProductName = "Empty", Ingredients = "" },
            new SephoraProduct { ProductName = "Null", Ingredients = null },
            new SephoraProduct { ProductName = "No Actives", Ingredients = "Water, Glycerin, Preservative" }
        };
        
        Assert.DoesNotThrow(() => _service.ParseParallel(edgeCases));
        
        Assert.Multiple(() =>
        {
            Assert.That(edgeCases[0].ParsedIngredients, Is.Empty);
            Assert.That(edgeCases[2].ParsedIngredients, Is.Empty, "Звичайні інгредієнти не мають вважатися активами");
        });
    }
    
    [Test]
    [TestCase("Salicylic Acid 0.5%", 0.5)] 
    [TestCase("Retinol 1%", 1.0)]
    [TestCase("Acid with no %", 0.0)]
    public void ExtractConcentration_ShouldWorkCorrectly(string ingredientText, double expected)
    {
        var product = new SephoraProduct { 
            ProductName = ingredientText, 
            Ingredients = ingredientText 
        };
    
        _service.ParseParallel(new List<SephoraProduct> { product });
        
        var ingredient = product.ParsedIngredients.FirstOrDefault();
        
        double actual = ingredient?.Concentration ?? 0.0;

        Assert.That(actual, Is.EqualTo(expected), $"Для '{ingredientText}' очікувалось {expected}%");
    }
    
    [Test]
    public void ParallelParsing_ShouldBeThreadSafe_UnderHeavyLoad()
    {
        int count = 5000;
        var bigData = Enumerable.Range(0, count).Select(i => new SephoraProduct
        {
            ProductName = $"Heavy Product {i}",
            Ingredients = "Water, Glycolic Acid 10%, Lactic Acid 5%"
        }).ToList();
        
        Assert.DoesNotThrow(() => _service.ParseParallel(bigData), 
            "Паралельна обробка великого обсягу даних не повинна викликати Race Condition");
        
        bool allProcessed = bigData.All(p => p.ParsedIngredients.Count == 2);
        Assert.That(allProcessed, Is.True, "Кожен продукт мав бути оброблений повністю без втрати даних у потоках");
    }
}