using Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/orders")]
public class OrdersController : ControllerBase
{
    private static readonly List<Order> Orders =
    [
        new() { Id = 1, UserId = 1, Product = "Laptop", Amount = 999.99m },
        new() { Id = 2, UserId = 2, Product = "Phone", Amount = 499.99m }
    ];

    [HttpGet]
    public List<Order> GetAll() => Orders;

    [HttpGet("{id}")]
    public Order? Get([FromRoute] string id)
    {
        return Orders.FirstOrDefault(o => o.Id == int.Parse(id));
    }

    [HttpPost]
    public Order Create([FromBody] Order order)
    {
        order.Id = Orders.Count > 0 ? Orders.Max(o => o.Id) + 1 : 1;
        order.CreatedAt = DateTime.UtcNow;
        Orders.Add(order);
        return order;
    }

    [HttpPut("{id}")]
    public Order? Update([FromRoute] string id, [FromBody] Order order)
    {
        var existing = Orders.FirstOrDefault(o => o.Id == int.Parse(id));
        if (existing is null) return null;
        existing.UserId = order.UserId;
        existing.Product = order.Product;
        existing.Amount = order.Amount;
        return existing;
    }

    [HttpDelete("{id}")]
    public bool Delete([FromRoute] string id)
    {
        var order = Orders.FirstOrDefault(o => o.Id == int.Parse(id));
        if (order is null) return false;
        Orders.Remove(order);
        return true;
    }
}
