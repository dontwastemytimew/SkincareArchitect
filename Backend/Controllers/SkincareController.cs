using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

/// <summary>
/// Контролер для керування аналізом косметичних засобів.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class SkincareController : ControllerBase
{
    private readonly SkincareFacade _facade;

    /// <summary>
    /// Фасад всередині тримає логер, стратегію та локалізатор.
    /// </summary>
    public SkincareController(SkincareFacade facade)
    {
        _facade = facade;
    }

    /// <summary>
    /// Тестовий метод.
    /// </summary>
    [HttpGet("test")]
    public IActionResult GetTest()
    {
        var p1 = new Product { Name = "Product 1" };
        var p2 = new Product { Name = "Product 2" };
        
        string message = _facade.SimpleCheck(p1, p2);

        return Ok(new { 
            status = "Active", 
            message = message 
        });
    }
    
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
    
    [HttpGet("test-conflict")]
    public IActionResult GetConflict()
    {
        var builder = new ProductBuilder();

        // Створюємо сироватку з Ретинолом
        var p1 = builder.Create("Retinol Serum")
            .AddIngredient("Pure Retinol", "Retinoid")
            .Build();

        // Створюємо пілінг з Кислотами
        var p2 = builder.Create("Acid Peel")
            .AddIngredient("Glycolic Acid", "Acid")
            .Build();

        string result = _facade.SimpleCheck(p1, p2);

        return Ok(new { 
            product1 = p1.Name, 
            product2 = p2.Name, 
            analysis = result 
        });
    }
    
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
    
    public class ProductViewModel {
        public string Name { get; set; }
        public string Type { get; set; }
    }
    
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