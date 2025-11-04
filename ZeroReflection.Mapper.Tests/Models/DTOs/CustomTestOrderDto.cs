namespace ZeroReflection.Mapper.Tests.Models.DTOs;

public class CustomTestOrderDto
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    //public List<CustomTestOrderItemDto> Items { get; set; } = new();
}
