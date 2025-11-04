namespace ZeroReflection.Mapper.Tests.Models.Entities;

public class ProductEntity
{
    public string ProductName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public List<ProductTagEntity> ProductTags { get; set; } = new();
}
