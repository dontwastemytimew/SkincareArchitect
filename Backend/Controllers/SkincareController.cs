using Backend.Services;
using Backend.Models;
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
        
        var routine = new Routine("User Analysis Routine");
        
        foreach (var fp in frontendProducts)
        {
            var product = builder.Create(fp.Name)
                .AddIngredient(fp.Name, fp.Type, fp.PH, fp.Concentration)
                .AddIngredient("Aqua", "Base", 7.0, 0.0) 
                .Build();
            routine.Add(product);
        }
        
        string report = _facade.SimpleCheck(routine); 

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
        
        /// <summary>Рівень кислотності (PH)</summary>
        public double PH { get; set; } = 5.5; 
        
        /// <summary>Концентрація активної речовини</summary>
        public double Concentration { get; set; } = 0.0;
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
            "AboutDescription", "FooterText", "BtnConfirm", "BtnCancel", "PlaceholderName", 
            "CatalogSub", "BtnToShelf", "MorningTitle", "EveningTitle", "RoutineHeader",
            "SearchConstructor", "SearchCatalog", "ConflictBarrier", "WarnIrritation", "ConflictBurn",
            "WarnTwoAcids", "WarnMissingConc", "CriticalConflictsHeader", "RecommendationsHeader", "IncompatibilityDetected",
            "LoginFirst", "AddedStatus", "EmptyShelfAlert", "AlreadyInQueue", "ClickToRemove", "MinProductsAlert",
            "LoadingText", "RoutineNotBuilt", "ServerError", "WelcomeUser", "NothingFound", "CatalogSearchPrompt",
            "AboutMissionTitle", "AboutMissionDesc", "AboutHowTitle", "AboutHowDesc", "AboutDisclaimer", "EmptyShelfText",
            "LoginTitle", "PlaceholderPass", "WrongPass", "FillAllFields", "RegisteredSuccess"
        };
        
        var translations = keys.ToDictionary(k => k, k => _facade.GetTranslation(k));
    
        return Ok(translations);
    }
    
    /// <summary>
    /// Отримує відфільтрований та оброблений список косметичних продуктів для каталогу.
    /// </summary>
    /// <returns>JSON-масив з об'єктами продуктів для відображення на фронтенді.</returns>
    [HttpGet("products")]
    public IActionResult GetProducts()
    {
        var dataService = new SephoraDataService("Data/product_info.csv"); 

        var rawProducts = dataService.LoadProducts();
        
        var filteredProducts = rawProducts
            .AsParallel()
            .Where(p => dataService.IsValidSkincare(p)) 
            .ToList();
        
        dataService.ParseParallel(filteredProducts);

        var result = filteredProducts.Take(300).Select(p => new {
            id = p.ProductId,
            name = p.ProductName,
            brand = p.BrandName,
            type = p.ParsedIngredients.FirstOrDefault()?.ActiveType ?? "Basic",
            ph = p.ParsedIngredients.FirstOrDefault()?.PHLevel ?? 5.5,
            concentration = p.ParsedIngredients.FirstOrDefault()?.Concentration ?? 0.0,
            order = p.ApplicationOrder,
            time = p.PreferredTime
        });

        return Ok(result);
    }
}