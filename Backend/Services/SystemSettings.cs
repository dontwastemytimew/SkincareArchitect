using Microsoft.Extensions.Logging;

namespace Backend.Services;

/// <summary>
/// Глобальні налаштування системи. Реалізує патерн Singleton.
/// </summary>
public sealed class SystemSettings
{
    private static SystemSettings? _instance;
    private static readonly object _lock = new object();
    private readonly ILogger<SystemSettings> _logger;

    /// <summary> Версія застосунку. </summary>
    public string Version { get; } = "1.0.0-beta";

    /// <summary> Час ініціалізації налаштувань. </summary>
    public DateTime InitializedAt { get; }
    
    private SystemSettings(ILogger<SystemSettings> logger)
    {
        _logger = logger;
        InitializedAt = DateTime.Now;
        _logger.LogInformation("Екземпляр SystemSettings (Singleton) успішно створено.");
    }

    /// <summary>
    /// Метод для отримання єдиного екземпляра класу.
    /// </summary>
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
}