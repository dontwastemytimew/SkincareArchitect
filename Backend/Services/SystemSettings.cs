namespace Backend.Services;

/// <summary>
/// Глобальні налаштування системи. 
/// Реалізує патерн <c>Singleton</c> для забезпечення єдиної точки доступу до конфігурації.
/// </summary>
/// <remarks>
/// Клас є <c>sealed</c>, що запобігає успадкуванню, та використовує механізм 
/// <c>double-check locking</c> для потокобезпечної ініціалізації.
/// </remarks>
public sealed class SystemSettings
{
    private static SystemSettings? _instance;
    private static readonly object _lock = new object();
    private readonly ILogger<SystemSettings> _logger;

    /// <summary> 
    /// Отримує поточну версію застосунку. 
    /// </summary>
    public string Version { get; } = "1.0.0-beta";

    /// <summary> 
    /// Отримує дату та час ініціалізації налаштувань. 
    /// </summary>
    public DateTime InitializedAt { get; }
    
    /// <summary>
    /// Приватний конструктор для запобігання створенню екземплярів ззовні.
    /// </summary>
    /// <param name="logger">Екземпляр логера для запису системних подій.</param>
    private SystemSettings(ILogger<SystemSettings> logger)
    {
        _logger = logger;
        InitializedAt = DateTime.Now;
        _logger.LogInformation("Екземпляр SystemSettings (Singleton) успішно створено.");
    }

    /// <summary>
    /// Повертає єдиний екземпляр класу <see cref="SystemSettings"/>.
    /// </summary>
    /// <param name="logger">Логер, що буде переданий у конструктор при першому зверненні.</param>
    /// <returns>Існуючий або новостворений об'єкт налаштувань.</returns>
    /// <remarks>
    /// Використовує блокування <c>lock</c>, що гарантує коректну роботу в багатопотоковому середовищі.
    /// </remarks>
    public static SystemSettings GetInstance(ILogger<SystemSettings> logger)
    {
        if (_instance == null)
        {
            lock (_lock)
            {
                if (_instance == null)
                {
                    _instance = new SystemSettings(logger);
                }
            }
        }
        return _instance;
    }
    
    /// <summary>
    /// Максимально допустима сумарна концентрація активів для безпечного домашнього догляду.
    /// </summary>
    public double MaxSafeConcentration { get; } = 5.0;

    /// <summary>
    /// Мінімальний поріг pH, нижче якого засоби вважаються агресивними.
    /// </summary>
    public double CriticalPHThreshold { get; } = 3.5;
}