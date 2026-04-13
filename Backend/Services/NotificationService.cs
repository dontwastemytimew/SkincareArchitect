namespace Backend.Services;

/// <summary>
/// Визначає інтерфейс для об'єктів, які мають отримувати сповіщення від суб'єкта.
/// Компонент патерну <c>Observer</c>.
/// </summary>
public interface IObserver
{
    /// <summary>
    /// Метод, який викликається суб'єктом для передачі оновленої інформації.
    /// </summary>
    /// <param name="message">Текст сповіщення про конфлікт або подію.</param>
    void Update(string message);
}

/// <summary>
/// Суб'єкт, який керує списком спостерігачів та розсилає їм повідомлення.
/// Патерн <c>Observer</c>.
/// </summary>
public class ConflictNotifier
{
    /// <summary> Список зареєстрованих спостерігачів. </summary>
    private readonly List<IObserver> _observers = new();

    /// <summary>
    /// Додає нового спостерігача до списку розсилки.
    /// </summary>
    /// <param name="observer">Об'єкт спостерігача.</param>
    public void Attach(IObserver observer) => _observers.Add(observer);

    /// <summary>
    /// Сповіщає всіх підписаних спостерігачів про подію.
    /// </summary>
    /// <param name="message">Повідомлення, яке буде передано всім спостерігачам.</param>
    public void Notify(string message)
    {
        foreach (var observer in _observers)
            observer.Update(message);
    }
}

/// <summary>
/// Конкретна реалізація спостерігача, яка записує сповіщення у системний лог.
/// </summary>
public class SecurityLogger : IObserver
{
    private readonly ILogger _logger;
    
    /// <summary>
    /// Ініціалізує новий екземпляр логера сповіщень.
    /// </summary>
    /// <param name="logger">Екземпляр системного логера.</param>
    public SecurityLogger(ILogger logger) => _logger = logger;

    /// <summary>
    /// Реалізація методу Update, що записує попередження у лог.
    /// </summary>
    /// <param name="message">Повідомлення про конфлікт складників.</param>
    public void Update(string message) 
    {
        _logger.LogWarning("OBSERVER NOTIFICATION: {Msg}", message);
    }
}