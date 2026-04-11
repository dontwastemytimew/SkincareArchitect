using System.Collections;

namespace Backend.Models;

/// <summary>
/// Патерн Composite. Набір засобів (рутина). Може містити як окремі продукти, так і інші набори.
/// </summary>
public class Routine : ISkincareComponent
{
    private readonly string _name;
    private readonly List<ISkincareComponent> _components = new();

    public Routine(string name) => _name = name;

    public void Add(ISkincareComponent component) => _components.Add(component);

    public string GetName() => _name;

    public List<Ingredient> GetAllIngredients() => 
        _components.SelectMany(c => c.GetAllIngredients()).ToList();
    
    public IEnumerator<ISkincareComponent> GetEnumerator() => _components.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}