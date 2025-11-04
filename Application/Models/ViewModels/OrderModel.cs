namespace Application.Models.ViewModels;

public class OrderModel
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public decimal TotalAmount { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public List<string> ProductNames { get; set; } = new List<string>();
    public string ShippingAddress { get; set; } = string.Empty;
    public string BillingAddress { get; set; } = string.Empty;
    public List<OrderItemModel> OrderItems { get; set; }
    
    public string? OrderStatus { get; set; } = "Pending"; // Default status
    public DateTime? ShippedDate { get; set; } = null; // Nullable
    public DateTime? DeliveredDate { get; set; } = null; // Nullable
    public string? TrackingNumber { get; set; } = null; // Nullable
    public string? PaymentMethod { get; set; } = null; // Nullable
    public string? Notes { get; set; } = null; // Nullable
    public string? CustomerPhone { get; set; } = null; // Nullable
    public string? CustomerAddress { get; set; } = null; // Nullable
    public string? CustomerCity { get; set; } = null; // Nullable
    public string? CustomerState { get; set; } = null; // Nullable
    public string? CustomerZipCode { get; set; } = null; // Nullable
    public string? CustomerCountry { get; set; } = null; // Nullable
    public string? CustomerCompany { get; set; } = null; // Nullable
    public string? CustomerTaxId { get; set; } = null; // Nullable
    
}