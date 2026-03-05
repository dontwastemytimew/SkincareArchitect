namespace Backend.Models;

/// <summary> Хімічний інгредієнт. </summary>
public class Ingredient
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ActiveType { get; set; } = string.Empty;
}