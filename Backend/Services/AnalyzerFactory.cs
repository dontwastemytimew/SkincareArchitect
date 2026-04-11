namespace Backend.Services;

/// <summary>
/// Абстрактний клас "Творець" для фабричного методу.
/// </summary>
public abstract class SkincareProvider
{
    public abstract ICompatibilityStrategy CreateStrategy();

    public bool PerformCheck(Models.Product p1, Models.Product p2)
    {
        var strategy = CreateStrategy();
        return strategy.Check(p1, p2);
    }
}

/// <summary>
/// Конкретна фабрика для базового аналізу.
/// </summary>
public class BasicProvider : SkincareProvider
{
    private readonly ILogger<SimpleCompatibilityStrategy> _logger;
    public BasicProvider(ILogger<SimpleCompatibilityStrategy> logger) => _logger = logger;

    public override ICompatibilityStrategy CreateStrategy() 
        => new SimpleCompatibilityStrategy(_logger);
}