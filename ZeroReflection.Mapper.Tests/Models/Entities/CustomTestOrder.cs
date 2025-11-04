namespace ZeroReflection.Mapper.Tests.Models.Entities;

public class CustomTestOrder
{
    public int Id { get; set; }
    public bool IsCompleted { get; set; }
    public bool IsCancelled { get; set; }
    public DateTime OrderDate { get; set; }
    public List<CustomTestOrderItem> Items { get; set; } = new();
    public CustomTestUser Customer { get; set; }
}
