namespace ZeroReflection.Mapper.Tests.Models.Entities;

public class CustomTestOrderItem
{
    public int Id { get; set; }
    public CustomTestProduct Product { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}
