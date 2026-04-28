using Backend.Models;
using Backend.Services;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace Backend.Tests;

[TestFixture]
public class MultithreadingTests
{
    [Test]
    public void ParallelParsing_YieldsSameResults_AsSequential()
    {
        var rawProducts = new List<SephoraProduct>
        {
            new SephoraProduct { ProductName = "Test 1", Ingredients = "Water, Retinol 1%, Glycerin" },
            new SephoraProduct { ProductName = "Test 2", Ingredients = "Salicylic Acid, Lactic Acid" },
            new SephoraProduct { ProductName = "Test 3", Ingredients = "Vitamin C, Fragrance" }
        };
        
        var listForSeq = rawProducts.Select(p => new SephoraProduct { ProductName = p.ProductName, Ingredients = p.Ingredients }).ToList();
        var listForPar = rawProducts.Select(p => new SephoraProduct { ProductName = p.ProductName, Ingredients = p.Ingredients }).ToList();

        var service = new SephoraDataService("");
        
        service.ParseSequential(listForSeq);
        service.ParseParallel(listForPar);
        
        int totalIngredientsSeq = listForSeq.Sum(p => p.ParsedIngredients.Count);
        int totalIngredientsPar = listForPar.Sum(p => p.ParsedIngredients.Count);
        
        Assert.Multiple(() =>
        {
            Assert.That(totalIngredientsPar, Is.EqualTo(totalIngredientsSeq), "Кількість розпізнаних інгредієнтів має співпадати");
            
            var seqRetinol = listForSeq[0].ParsedIngredients.FirstOrDefault(i => i.Name.Contains("Retinol"));
            var parRetinol = listForPar[0].ParsedIngredients.FirstOrDefault(i => i.Name.Contains("Retinol"));
            
            Assert.That(parRetinol, Is.Not.Null, "Паралельна версія мала знайти Ретинол");
            Assert.That(parRetinol?.ActiveType, Is.EqualTo(seqRetinol?.ActiveType), "Типи активів мають співпадати");
        });
    }
}