namespace Backend.Models;

/// <summary> 
/// Представляє хімічний інгредієнт, що входить до складу косметичного засобу. 
/// </summary>
public class Ingredient
{
    /// <summary> Унікальний ідентифікатор інгредієнта. </summary>
    public int Id { get; set; }
    
    /// <summary> Назва інгредієнта (наприклад, Retinol, Salicylic Acid). </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary> Опис властивостей інгредієнта та його впливу на шкіру. </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary> Тип активної речовини (Acid, Retinoid, VitaminC тощо) для перевірки сумісності. </summary>
    public string ActiveType { get; set; } = string.Empty;
}