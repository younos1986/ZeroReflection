using Microsoft.Extensions.DependencyInjection;
using ZeroReflection.Mapper;
using ZeroReflection.Mapper.Generated;
using ZeroReflection.Mapper.Tests.Models.Entities;
using ZeroReflection.Mapper.Tests.Models.DTOs;

namespace ZeroReflection.Mapper.Tests.CustomMappers;

// Product-specific mapping profile
public class ProductMappingProfile : MapperProfile
{
    public override void Configure(MapperConfiguration config)
    {
        config.EnableProjectionFunctions = true;
        
    }

    // Complete custom mapping using attributes
    [CustomMapping(typeof(CustomTestProduct), typeof(CustomTestProductDto))]
    public static CustomTestProductDto MapProductToDto(CustomTestProduct source)
    {
        return new CustomTestProductDto
        {
            Id = source.Id,
            Name = source.Name,
            Price = source.Price,
            CategoryName = source.Category?.Name ?? "Unknown",
            DiscountedPrice = CalculateDiscountedPrice(source),
            DisplayName = FormatProductDisplayName(source)
        };
    }

    private static decimal CalculateDiscountedPrice(CustomTestProduct product)
    {
        // Custom business logic for calculating discounts
        if (product.Category?.Name == "Electronics")
            return product.Price * 0.9m; // 10% discount
        return product.Price;
    }

    private static string FormatProductDisplayName(CustomTestProduct product)
    {
        return $"{product.Name} ({product.Category?.Name})";
    }
}

public class ProductModelTests
{
    private readonly IMapper _mapper;

    public ProductModelTests()
    {
        // Arrange
        var services = new ServiceCollection();
        services.RegisterZeroReflectionMapping();
        
        var serviceProvider = services.BuildServiceProvider();
        _mapper = serviceProvider.GetRequiredService<IMapper>();
    }

    [Fact]
    public void Should_Map_Product_With_Custom_Mapping_Attribute()
    {
        // Arrange
        var category = new CustomTestCategory
        {
            Id = 1,
            Name = "Electronics"
        };

        var product = new CustomTestProduct
        {
            Id = 200,
            Name = "Smartphone",
            Price = 800m,
            Category = category
        };

        // Act
        var productDto = _mapper.MapSingleObject<CustomTestProduct, CustomTestProductDto>(product);

        // Assert
        Assert.NotNull(productDto);
        Assert.Equal(200, productDto.Id);
        Assert.Equal("Smartphone", productDto.Name);
        Assert.Equal(800m, productDto.Price);
        Assert.Equal("Electronics", productDto.CategoryName);
        Assert.Equal(720m, productDto.DiscountedPrice); // 10% discount for Electronics
        Assert.Equal("Smartphone (Electronics)", productDto.DisplayName);
    }

    [Fact]
    public void Should_Apply_Electronics_Discount()
    {
        // Arrange
        var electronicsCategory = new CustomTestCategory
        {
            Id = 1,
            Name = "Electronics"
        };

        var product = new CustomTestProduct
        {
            Id = 100,
            Name = "Laptop",
            Price = 1000m,
            Category = electronicsCategory
        };

        // Act
        var productDto = _mapper.MapSingleObject<CustomTestProduct, CustomTestProductDto>(product);

        // Assert
        Assert.Equal(900m, productDto.DiscountedPrice); // 10% discount applied
    }

    [Fact]
    public void Should_Not_Apply_Discount_For_Non_Electronics()
    {
        // Arrange
        var booksCategory = new CustomTestCategory
        {
            Id = 2,
            Name = "Books"
        };

        var product = new CustomTestProduct
        {
            Id = 101,
            Name = "Programming Book",
            Price = 50m,
            Category = booksCategory
        };

        // Act
        var productDto = _mapper.MapSingleObject<CustomTestProduct, CustomTestProductDto>(product);

        // Assert
        Assert.Equal(50m, productDto.DiscountedPrice); // No discount applied
        Assert.Equal("Programming Book (Books)", productDto.DisplayName);
    }

    [Fact]
    public void Should_Handle_Product_With_No_Category()
    {
        // Arrange
        var product = new CustomTestProduct
        {
            Id = 102,
            Name = "Mystery Item",
            Price = 25m,
            Category = null
        };

        // Act
        var productDto = _mapper.MapSingleObject<CustomTestProduct, CustomTestProductDto>(product);

        // Assert
        Assert.NotNull(productDto);
        Assert.Equal("Unknown", productDto.CategoryName);
        Assert.Equal(25m, productDto.DiscountedPrice); // No discount for unknown category
        Assert.Equal("Mystery Item ()", productDto.DisplayName);
    }
}
