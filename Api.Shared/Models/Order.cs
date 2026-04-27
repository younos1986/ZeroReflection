namespace Api.Models;

public class Order
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Product { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
