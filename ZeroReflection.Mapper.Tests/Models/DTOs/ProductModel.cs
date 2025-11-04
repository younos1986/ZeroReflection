namespace ZeroReflection.Mapper.Tests.Models.DTOs;

public class ProductModel
{
    public string ProductName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public List<ProductTag> ProductTags { get; set; } = new();
}
