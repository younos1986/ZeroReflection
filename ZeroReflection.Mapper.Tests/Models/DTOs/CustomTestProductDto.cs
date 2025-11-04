namespace ZeroReflection.Mapper.Tests.Models.DTOs;

public class CustomTestProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public decimal DiscountedPrice { get; set; }
    public string DisplayName { get; set; } = string.Empty;
}
