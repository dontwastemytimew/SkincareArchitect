using Backend.Models;
using NUnit.Framework;

namespace Backend.Tests;

[TestFixture]
public class CompositeTests
{
    [Test]
    public void Routine_GetAllIngredients_CollectsFromAllSubProducts()
    {
        var p1 = new Product { Name = "P1" };
        p1.Ingredients.Add(new Ingredient { ActiveType = "Type1" });
        
        var p2 = new Product { Name = "P2" };
        p2.Ingredients.Add(new Ingredient { ActiveType = "Type2" });

        var routine = new Routine("Evening Routine");
        routine.Add(p1);
        routine.Add(p2);

        var allIngredients = routine.GetAllIngredients();

        Assert.That(allIngredients.Count, Is.EqualTo(2));
    }
    
    // Composite передбачає вкладеність: рутина в рутині.
    [Test]
    public void Routine_NestedRoutines_CollectsAllIngredients()
    {
        var mainRoutine = new Routine("Main");
        var subRoutine = new Routine("Sub");
    
        var p1 = new Product { Name = "P1" };
        p1.Ingredients.Add(new Ingredient { Name = "Ing1" });
    
        subRoutine.Add(p1);
        mainRoutine.Add(subRoutine);

        var ingredients = mainRoutine.GetAllIngredients();
    
        Assert.That(ingredients.Count, Is.EqualTo(1), "Має знаходити інгредієнти навіть у вкладених рутинах");
    }
    
    [Test]
    public void Routine_Iterator_CanTraverseProducts()
    {
        var routine = new Routine("Test");
        routine.Add(new Product { Name = "A" });
        routine.Add(new Product { Name = "B" });
        
        int count = 0;
        foreach (var item in routine)
        {
            count++;
        }
        
        Assert.That(count, Is.EqualTo(2), "Ітератор має пройти по обох продуктах у рутині");
    }
}