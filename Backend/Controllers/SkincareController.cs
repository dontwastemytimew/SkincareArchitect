using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

/// <summary>
/// API-контролер для обробки запитів, пов'язаних з аналізом косметики та локалізацією інтерфейсу.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class SkincareController : ControllerBase
{
    private readonly SkincareFacade _facade;

    /// <summary>
    /// Конструктор контролера.
    /// </summary>
    /// <param name="facade">Фасад, що об'єднує логіку перевірки сумісності та локалізації.</param>
    public SkincareController(SkincareFacade facade)
    {
        _facade = facade;
    }
    
    /// <summary>
    /// Отримує системну інформацію про додаток (версія, час запуску).
    /// Використовує патерн Singleton через SystemSettings.
    /// </summary>
    /// <returns>Дані про стан сервера.</returns>
    [HttpGet("info")]
    public IActionResult GetInfo()
    {
        var settings = HttpContext.RequestServices.GetRequiredService<SystemSettings>();

        return Ok(new {
            AppName = "Skincare Architect",
            Version = settings.Version,
            ServerStartedAt = settings.InitializedAt, 
            CurrentTime = DateTime.Now
        });
    }
    
    /// <summary>
    /// Основний метод аналізу списку засобів, отриманих з фронтенду.
    /// </summary>
    /// <param name="frontendProducts">Список спрощених моделей продуктів (JSON).</param>
    /// <returns>Результат перевірки сумісності першої пари засобів.</returns>
    [HttpPost("analyze")]
    public IActionResult Analyze([FromBody] List<ProductViewModel> frontendProducts)
    {
        
        if (frontendProducts == null || frontendProducts.Count < 2) 
            return BadRequest("Додайте хоча б два засоби");
        
        var builder = new ProductBuilder();
        var products = frontendProducts.Select(fp => 
            builder.Create(fp.Name)
                .AddIngredient(fp.Name, fp.Type)
                .Build()
        ).ToList();
        
        string report = _facade.SimpleCheck(products[0], products[1]);
    
        return Ok(new { analysis = report });
    }
    
    /// <summary>
    /// Допоміжна модель (DTO) для отримання даних про продукт з фронтенду.
    /// </summary>
    public class ProductViewModel {
        /// <summary>Назва засобу</summary>
        public string Name { get; set; }
        /// <summary>Тип активного інгредієнта (Retinoid, Acid, Moisturizer тощо)</summary>
        public string Type { get; set; }
    }
    
    /// <summary>
    /// Отримує словник перекладів для елементів інтерфейсу на основі заголовка Accept-Language.
    /// </summary>
    /// <returns>Словник ключ-значення для локалізації фронтенду.</returns>
    [HttpGet("translations")]
    public IActionResult GetTranslations()
    {
        var keys = new[] { 
            "NavConstructor", "NavShelf", "NavCatalog", "NavAbout", "AuthLogin", 
            "SourceAll", "SourceShelf", "BtnAddAnalysis", "BtnRunScan", "BtnBack",
            "HeaderOverlay", "TitleConstructor", "TitleAnalysis", "SelectPlaceholder",
            "AboutDescription", "FooterText" 
        };
        
        var translations = keys.ToDictionary(k => k, k => _facade.GetTranslation(k));
    
        return Ok(translations);
    }
}