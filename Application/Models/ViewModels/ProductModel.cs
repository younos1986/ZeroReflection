namespace Application.Models.ViewModels;

public class ProductModel
{
    public string Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public string Category { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string? ImageUrl { get; set; }   
    public string? Manufacturer { get; set; }
    public string? Barcode { get; set; }
    public string? Tags { get; set; } 
    public bool IsActive { get; set; } 
}