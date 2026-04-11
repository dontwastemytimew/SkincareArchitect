namespace Backend.Services;

public interface IObserver
{
    void Update(string message);
}

/// <summary>
/// Суб'єкт, який розсилає сповіщення.
/// Патерн Observer.
/// </summary>
public class ConflictNotifier
{
    private readonly List<IObserver> _observers = new();

    public void Attach(IObserver observer) => _observers.Add(observer);

    public void Notify(string message)
    {
        foreach (var observer in _observers)
            observer.Update(message);
    }
}

/// <summary>
/// Конкретний спостерігач — логер безпеки.
/// </summary>
public class SecurityLogger : IObserver
{
    private readonly ILogger _logger;
    public SecurityLogger(ILogger logger) => _logger = logger;

    public void Update(string message) 
    {
        _logger.LogWarning("OBSERVER NOTIFICATION: {Msg}", message);
    }
}