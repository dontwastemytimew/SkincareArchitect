using System.Collections;

namespace Backend.Models;

/// <summary>
/// Представляє набір косметичних засобів (комплексну рутину).
/// Виступає як "Контейнер" (Composite) у структурі патерна Composite.
/// </summary>
public class Routine : ISkincareComponent
{
    private readonly string _name;
    private readonly List<ISkincareComponent> _components = new();

    /// <summary> Ініціалізує новий екземпляр класу Routine з назвою. </summary>
    /// <param name="name">Назва набору (наприклад, "Ранковий догляд").</param>
    public Routine(string name) => _name = name;

    /// <summary> Додає новий засіб або інший набір засобів до поточної рутини. </summary>
    /// <param name="component">Компонент догляду (Product або Routine).</param>
    public void Add(ISkincareComponent component) => _components.Add(component);

    /// <inheritdoc/>
    public string GetName() => _name;

    /// <summary> Збирає докупи всі інгредієнти з усіх продуктів, що додані в рутину. </summary>
    /// <returns>Плоский список усіх інгредієнтів.</returns>
    public List<Ingredient> GetAllIngredients() => 
        _components.SelectMany(c => c.GetAllIngredients()).ToList();
    
    /// <summary> Реалізація патерна Iterator для проходу по всім компонентам рутини. </summary>
    public IEnumerator<ISkincareComponent> GetEnumerator() => _components.GetEnumerator();
    
    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}