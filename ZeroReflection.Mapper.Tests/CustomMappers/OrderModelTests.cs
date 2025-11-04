using Microsoft.Extensions.DependencyInjection;
using ZeroReflection.Mapper;
using ZeroReflection.Mapper.Generated;
using ZeroReflection.Mapper.Tests.Models.Entities;
using ZeroReflection.Mapper.Tests.Models.DTOs;
using System.Linq;

namespace ZeroReflection.Mapper.Tests.CustomMappers;

public class OrderMappingProfile : MapperProfile
{
    public override void Configure(MapperConfiguration config)
    {
        config.EnableProjectionFunctions = true;
        
        // Custom mapping using WithCustomMapping
        config.CreateMap<CustomTestOrder, CustomTestOrderDto>()
            .WithCustomMapping(MapOrderToDto);
        
        // Regular mapping for items
        //config.CreateMap<CustomTestOrderItem, CustomTestOrderItemDto>();
    }

    [CustomPropertyMapping(typeof(CustomTestOrder), typeof(CustomTestOrderDto), "Status")]
    private string GetOrderStatus(CustomTestOrder order)
    {
        return order.IsCompleted ? "Completed" : 
               order.IsCancelled ? "Cancelled" : "Pending";
    }

    // Complex custom mapping method
    internal static CustomTestOrderDto MapOrderToDto(CustomTestOrder source)
    {
        return new CustomTestOrderDto
        {
            Id = source.Id,
            OrderNumber = $"ORD-{source.Id:D6}",
            //Items = source.Items?.Select(MapOrderItemToDto).ToList() ?? new List<CustomTestOrderItemDto>(),
            TotalAmount = source.Items?.Sum(i => i.Price * i.Quantity) ?? 0,
            CustomerName = $"{source.Customer?.FirstName} {source.Customer?.LastName}".Trim(),
            Status = source.IsCompleted ? "Completed" : source.IsCancelled ? "Cancelled" : "Pending"
        };
    }

    // private static CustomTestOrderItemDto MapOrderItemToDto(CustomTestOrderItem item)
    // {
    //     return new CustomTestOrderItemDto
    //     {
    //         ProductName = item.Product?.Name ?? "Unknown",
    //         Quantity = item.Quantity,
    //         UnitPrice = item.Price,
    //         TotalPrice = item.Price * item.Quantity
    //     };
    // }
}

public class OrderModelTests
{
    private readonly IMapper _mapper;

    public OrderModelTests()
    {
        // Arrange
        var services = new ServiceCollection();
        services.RegisterZeroReflectionMapping();
        
        var serviceProvider = services.BuildServiceProvider();
        _mapper = serviceProvider.GetRequiredService<IMapper>();
    }

    [Fact]
    public void Should_Map_Order_With_Custom_Mapping_Method()
    {
        // Arrange
        var customer = new CustomTestUser
        {
            Id = 1,
            FirstName = "Jane",
            LastName = "Smith"
        };

        var product = new CustomTestProduct
        {
            Id = 100,
            Name = "Laptop",
            Price = 1000m
        };

        var orderItem = new CustomTestOrderItem
        {
            Id = 1,
            Product = product,
            Quantity = 2,
            Price = 1000m
        };

        var order = new CustomTestOrder
        {
            Id = 456,
            IsCompleted = true,
            IsCancelled = false,
            OrderDate = DateTime.Now,
            Customer = customer,
            Items = new List<CustomTestOrderItem> { orderItem }
        };

        // Act
        var orderDto = _mapper.MapSingleObject<CustomTestOrder, CustomTestOrderDto>(order);

        // Assert
        Assert.NotNull(orderDto);
        Assert.Equal(456, orderDto.Id);
        Assert.Equal("ORD-000456", orderDto.OrderNumber); // Custom formatting
        Assert.Equal("Completed", orderDto.Status); // Custom status logic
        Assert.Equal(2000m, orderDto.TotalAmount); // Calculated total
        Assert.Equal("Jane Smith", orderDto.CustomerName); // Custom customer name formatting
       // Assert.Single(orderDto.Items);
        
       // var itemDto = orderDto.Items.First();
        // Assert.Equal("Laptop", itemDto.ProductName);
        // Assert.Equal(2, itemDto.Quantity);
        // Assert.Equal(1000m, itemDto.UnitPrice);
        // Assert.Equal(2000m, itemDto.TotalPrice);
    }

    [Fact]
    public void Should_Handle_Order_Status_Custom_Property_Mapping()
    {
        // Arrange - Cancelled order
        var cancelledOrder = new CustomTestOrder
        {
            Id = 789,
            IsCompleted = false,
            IsCancelled = true,
            Items = new List<CustomTestOrderItem>()
        };

        // Act
        var orderDto = _mapper.MapSingleObject<CustomTestOrder, CustomTestOrderDto>(cancelledOrder);

        // Assert
        Assert.Equal("Cancelled", orderDto.Status);
        
        // Arrange - Pending order
        var pendingOrder = new CustomTestOrder
        {
            Id = 790,
            IsCompleted = false,
            IsCancelled = false,
            Items = new List<CustomTestOrderItem>()
        };

        // Act
        var pendingOrderDto = _mapper.MapSingleObject<CustomTestOrder, CustomTestOrderDto>(pendingOrder);

        // Assert
        Assert.Equal("Pending", pendingOrderDto.Status);
    }

    [Fact]
    public void Should_Handle_Null_Values_In_Custom_Mappings()
    {
        // Arrange
        var order = new CustomTestOrder
        {
            Id = 999,
            IsCompleted = false,
            IsCancelled = false,
            Items = null, // null items
            Customer = null // null customer
        };

        // Act
        var orderDto = _mapper.MapSingleObject<CustomTestOrder, CustomTestOrderDto>(order);

        // Assert
        Assert.NotNull(orderDto);
        Assert.Equal(999, orderDto.Id);
        Assert.Equal("ORD-000999", orderDto.OrderNumber);
        Assert.Equal("Pending", orderDto.Status);
        Assert.Equal(0m, orderDto.TotalAmount); // Should handle null items gracefully
        Assert.Equal(string.Empty, orderDto.CustomerName.Trim()); // Should handle null customer gracefully
        //Assert.Empty(orderDto.Items); // Should return empty list instead of null
    }

    [Fact]
    public void Should_Calculate_Order_Total_Correctly()
    {
        // Arrange
        var product1 = new CustomTestProduct { Id = 1, Name = "Product1", Price = 100m };
        var product2 = new CustomTestProduct { Id = 2, Name = "Product2", Price = 50m };

        var order = new CustomTestOrder
        {
            Id = 123,
            IsCompleted = false,
            IsCancelled = false,
            Items = new List<CustomTestOrderItem>
            {
                new() { Product = product1, Quantity = 3, Price = 100m },
                new() { Product = product2, Quantity = 2, Price = 50m }
            }
        };

        // Act
        var orderDto = _mapper.MapSingleObject<CustomTestOrder, CustomTestOrderDto>(order);

        // Assert
        Assert.Equal(400m, orderDto.TotalAmount); // (3 * 100) + (2 * 50) = 400
       // Assert.Equal(2, orderDto.Items.Count);
    }
}
