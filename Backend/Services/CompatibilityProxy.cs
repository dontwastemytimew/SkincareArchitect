using Backend.Models;

namespace Backend.Services;

/// <summary>
/// Патерн Proxy. Замісник для кешування результатів перевірки сумісності.
/// </summary>
public class CompatibilityProxy : ICompatibilityStrategy
{
    private readonly ICompatibilityStrategy _realStrategy;
    private readonly Dictionary<string, bool> _cache = new();

    public CompatibilityProxy(ICompatibilityStrategy realStrategy)
    {
        _realStrategy = realStrategy;
    }

    public bool Check(Product p1, Product p2)
    {
        string key = $"{p1.Name}_{p2.Name}";

        if (_cache.ContainsKey(key))
        {
            return _cache[key];
        }

        bool result = _realStrategy.Check(p1, p2);
        _cache[key] = result;
        return result;
    }
}