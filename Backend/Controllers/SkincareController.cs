using Backend.Models;
using Backend.Services;
using Backend.Resources;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace Backend.Controllers;

/// <summary>
/// Контролер для керування аналізом косметичних засобів.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class SkincareController : ControllerBase
{
    private readonly ICompatibilityStrategy _strategy;
    private readonly ILogger<SkincareController> _logger;
    private readonly IStringLocalizer<SharedResource> _localizer;

    /// <summary>
    /// Конструктор з впровадженням залежностей.
    /// </summary>
    public SkincareController(
        ICompatibilityStrategy strategy, 
        ILogger<SkincareController> logger,
        IStringLocalizer<SharedResource> localizer)
    {
        _strategy = strategy;
        _logger = logger;
        _localizer = localizer;
    }

    /// <summary>
    /// Тестовий метод для перевірки роботи системи.
    /// </summary>
    [HttpGet("test")]
    public IActionResult GetTest()
    {
        var currentCulture = System.Globalization.CultureInfo.CurrentCulture.Name;
        _logger.LogInformation("Current Culture: {Culture}", currentCulture);

        string message = Backend.Resources.SharedResource.Greeting; 

        return Ok(new { 
            status = "Active", 
            detectedCulture = currentCulture,
            message = message 
        });
    }
}