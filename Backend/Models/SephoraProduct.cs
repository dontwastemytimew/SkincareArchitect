using CsvHelper.Configuration.Attributes;

namespace Backend.Models;

/// <summary>
/// DTO (Data Transfer Object) для мапінгу даних із зовнішнього датасету (Kaggle).
/// Використовується для моделювання обробки бази косметики.
/// </summary>
public class SephoraProduct
{
    /// <summary> Унікальний ідентифікатор продукту в базі магазину. </summary>
    [Name("product_id")]
    public string ProductId { get; set; } = string.Empty;

    /// <summary> Повна назва косметичного засобу. </summary>
    [Name("product_name")]
    public string ProductName { get; set; } = string.Empty;

    /// <summary> Назва бренду виробника. </summary>
    [Name("brand_name")]
    public string BrandName { get; set; } = string.Empty;
    
    /// <summary> 
    /// Рядок з переліком інгредієнтів. 
    /// Є об'єктом для ресурсомісткого парсингу.
    /// </summary>
    [Name("ingredients")]
    public string Ingredients { get; set; } = string.Empty;
    
    /// <summary> 
    /// Список структурованих об'єктів Ingredient, отриманих після обробки.
    /// Атрибут [Ignore] вказує парсеру CsvHelper пропустити це поле при читанні файлу.
    /// </summary>
    [Ignore]
    public List<Ingredient> ParsedIngredients { get; set; } = new();
}