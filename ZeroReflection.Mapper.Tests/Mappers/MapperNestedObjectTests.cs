using Microsoft.Extensions.DependencyInjection;
using ZeroReflection.Mapper.Generated;
using ZeroReflection.Mapper.Tests.Models.DTOs;
using ZeroReflection.Mapper.Tests.Models.Entities;

namespace ZeroReflection.Mapper.Tests.Mappers;

public class UserProfile : MapperProfile
{
    public override void Configure(MapperConfiguration config)
    {
        config.CreateMap<UserModel, UserEntity>().Reverse();
    }
}

public class MapperNestedObjectTests
{
    private readonly IMapper _mapper;

    public MapperNestedObjectTests()
    {
        var services = new ServiceCollection();
        services.RegisterZeroReflectionMapping();

        var serviceProvider = services.BuildServiceProvider();
        _mapper = serviceProvider.GetRequiredService<IMapper>();
    }

    [Fact]
    public void Should_Map_User_With_Nested_Address()
    {
        // Arrange
        var UserModel = new UserModel
        {
            Name = "John Doe",
            Age = 30,
            Addresses = new AddressModel
            {
                Street = "123 Main St",
                City = "New York"
            }
        };

        // Act
        var userEntity = _mapper.MapSingleObject<UserModel, UserEntity>(UserModel);

        // Assert
        Assert.NotNull(userEntity);
        Assert.Equal(UserModel.Name, userEntity.Name);
        Assert.Equal(UserModel.Age, userEntity.Age);
        Assert.NotNull(userEntity.Addresses);
        Assert.Equal(UserModel.Addresses.Street, userEntity.Addresses.Street);
        Assert.Equal(UserModel.Addresses.City, userEntity.Addresses.City);
    }

    [Fact]
    public void Should_Map_User_With_Product_Collection()
    {
        // Arrange
        var UserModel = new UserModel
        {
            Name = "Jane Smith",
            Age = 25,
            Products = new List<ProductModel>
            {
                new() { ProductName = "Laptop", Price = 999.99m },
                new() { ProductName = "Mouse", Price = 29.99m },
                new() { ProductName = "Keyboard", Price = 79.99m }
            }
        };

        // Act
        var userEntity = _mapper.MapSingleObject<UserModel, UserEntity>(UserModel);

        // Assert
        Assert.NotNull(userEntity);
        Assert.Equal(UserModel.Name, userEntity.Name);
        Assert.Equal(UserModel.Age, userEntity.Age);
        Assert.NotNull(userEntity.Products);
        Assert.Equal(3, userEntity.Products.Count);

        for (int i = 0; i < UserModel.Products.Count; i++)
        {
            Assert.Equal(UserModel.Products[i].ProductName, userEntity.Products[i].ProductName);
            Assert.Equal(UserModel.Products[i].Price, userEntity.Products[i].Price);
        }
    }

    [Fact]
    public void Should_Map_Complex_User_With_All_Nested_Objects()
    {
        // Arrange
        var UserModel = new UserModel
        {
            Name = "Bob Johnson",
            Age = 35,
            Addresses = new AddressModel
            {
                Street = "456 Oak Ave",
                City = "Los Angeles"
            },
            Products = new List<ProductModel>
            {
                new() { ProductName = "Phone", Price = 599.99m },
                new() { ProductName = "Case", Price = 19.99m }
            }
        };

        // Act
        var userEntity = _mapper.MapSingleObject<UserModel, UserEntity>(UserModel);

        // Assert
        Assert.NotNull(userEntity);
        Assert.Equal(UserModel.Name, userEntity.Name);
        Assert.Equal(UserModel.Age, userEntity.Age);

        // Verify nested address
        Assert.NotNull(userEntity.Addresses);
        Assert.Equal(UserModel.Addresses.Street, userEntity.Addresses.Street);
        Assert.Equal(UserModel.Addresses.City, userEntity.Addresses.City);

        // Verify product collection
        Assert.NotNull(userEntity.Products);
        Assert.Equal(2, userEntity.Products.Count);
        Assert.Equal("Phone", userEntity.Products[0].ProductName);
        Assert.Equal(599.99m, userEntity.Products[0].Price);
        Assert.Equal("Case", userEntity.Products[1].ProductName);
        Assert.Equal(19.99m, userEntity.Products[1].Price);
    }

    [Fact]
    public void Should_Map_Reverse_Entity_To_Dto()
    {
        // Arrange
        var userEntity = new UserEntity
        {
            Name = "Alice Brown",
            Age = 28,
            Addresses = new AddressEntity
            {
                Street = "789 Pine St",
                City = "Chicago"
            },
            Products = new List<ProductEntity>
            {
                new() { ProductName = "Tablet", Price = 299.99m }
            }
        };

        // Act
        var UserModel = _mapper.MapSingleObject<UserEntity, UserModel>(userEntity);

        // Assert
        Assert.NotNull(UserModel);
        Assert.Equal(userEntity.Name, UserModel.Name);
        Assert.Equal(userEntity.Age, UserModel.Age);
        Assert.NotNull(UserModel.Addresses);
        Assert.Equal(userEntity.Addresses.Street, UserModel.Addresses.Street);
        Assert.Equal(userEntity.Addresses.City, UserModel.Addresses.City);
        Assert.NotNull(UserModel.Products);
        Assert.Single(UserModel.Products);
        Assert.Equal("Tablet", UserModel.Products[0].ProductName);
        Assert.Equal(299.99m, UserModel.Products[0].Price);
    }

    [Fact]
    public void Should_Handle_Null_Nested_Objects()
    {
        // Arrange
        var UserModel = new UserModel
        {
            Name = "Test User",
            Age = 20,
            Addresses = null,
            Products = null
        };

        // Act
        var userEntity = _mapper.MapSingleObject<UserModel, UserEntity>(UserModel);

        // Assert
        Assert.NotNull(userEntity);
        Assert.Equal(UserModel.Name, userEntity.Name);
        Assert.Equal(UserModel.Age, userEntity.Age);
        Assert.Null(userEntity.Addresses);
        Assert.Null(userEntity.Products);
    }

    [Fact]
    public void Should_Handle_Empty_Product_Collection()
    {
        // Arrange
        var UserModel = new UserModel
        {
            Name = "Empty User",
            Age = 22,
            Products = new List<ProductModel>()
        };

        // Act
        var userEntity = _mapper.MapSingleObject<UserModel, UserEntity>(UserModel);

        // Assert
        Assert.NotNull(userEntity);
        Assert.Equal(UserModel.Name, userEntity.Name);
        Assert.Equal(UserModel.Age, userEntity.Age);
        Assert.NotNull(userEntity.Products);
        Assert.Empty(userEntity.Products);
    }

    [Fact]
    public void Should_Map_Collection_Of_Users_With_Nested_Objects()
    {
        // Arrange
        var UserModels = new List<UserModel>
        {
            new()
            {
                Name = "User 1",
                Age = 25,
                Addresses = new AddressModel { Street = "Street 1", City = "City 1" },
                Products = new List<ProductModel>
                {
                    new() { ProductName = "Product 1", Price = 100m }
                }
            },
            new()
            {
                Name = "User 2",
                Age = 30,
                Addresses = new AddressModel { Street = "Street 2", City = "City 2" },
                Products = new List<ProductModel>
                {
                    new() { ProductName = "Product 2", Price = 200m },
                    new() { ProductName = "Product 3", Price = 300m }
                }
            }
        };

        // Act
        var userEntities = _mapper.Map<List<UserEntity>>(UserModels);

        // Assert
        Assert.NotNull(userEntities);
        Assert.Equal(2, userEntities.Count);

        // Verify first user
        Assert.Equal("User 1", userEntities[0].Name);
        Assert.Equal(25, userEntities[0].Age);
        Assert.Equal("Street 1", userEntities[0].Addresses.Street);
        Assert.Equal("City 1", userEntities[0].Addresses.City);
        Assert.Single(userEntities[0].Products);
        Assert.Equal("Product 1", userEntities[0].Products[0].ProductName);

        // Verify second user
        Assert.Equal("User 2", userEntities[1].Name);
        Assert.Equal(30, userEntities[1].Age);
        Assert.Equal("Street 2", userEntities[1].Addresses.Street);
        Assert.Equal("City 2", userEntities[1].Addresses.City);
        Assert.Equal(2, userEntities[1].Products.Count);
        Assert.Equal("Product 2", userEntities[1].Products[0].ProductName);
        Assert.Equal("Product 3", userEntities[1].Products[1].ProductName);
    }

    [Fact]
    public void Should_Map_Individual_Address()
    {
        // Arrange
        var AddressModel = new AddressModel
        {
            Street = "123 Test Street",
            City = "Test City"
        };

        // Act
        var addressEntity = _mapper.MapSingleObject<AddressModel, AddressEntity>(AddressModel);

        // Assert
        Assert.NotNull(addressEntity);
        Assert.Equal(AddressModel.Street, addressEntity.Street);
        Assert.Equal(AddressModel.City, addressEntity.City);
    }

    [Fact]
    public void Should_Map_Individual_Product()
    {
        // Arrange
        var ProductModel = new ProductModel
        {
            ProductName = "Test Product",
            Price = 49.99m
        };

        // Act
        var productEntity = _mapper.MapSingleObject<ProductModel, ProductEntity>(ProductModel);

        // Assert
        Assert.NotNull(productEntity);
        Assert.Equal(ProductModel.ProductName, productEntity.ProductName);
        Assert.Equal(ProductModel.Price, productEntity.Price);
    }

    [Fact]
    public void Should_Map_Product_Collection_Separately()
    {
        // Arrange
        var ProductModels = new List<ProductModel>
        {
            new() { ProductName = "Product A", Price = 10.99m },
            new() { ProductName = "Product B", Price = 20.99m },
            new() { ProductName = "Product C", Price = 30.99m }
        };

        // Act
        var productEntities = _mapper.Map<List<ProductEntity>>(ProductModels);

        // Assert
        Assert.NotNull(productEntities);
        Assert.Equal(3, productEntities.Count);

        for (int i = 0; i < ProductModels.Count; i++)
        {
            Assert.Equal(ProductModels[i].ProductName, productEntities[i].ProductName);
            Assert.Equal(ProductModels[i].Price, productEntities[i].Price);
        }
    }

    [Fact]
    public void Should_Map_Individual_ProductTag()
    {
        // Arrange
        var productTag = new ProductTag
        {
            TagName = "Category",
            TagValue = "Electronics"
        };

        // Act
        var productTagEntity = _mapper.MapSingleObject<ProductTag, ProductTagEntity>(productTag);

        // Assert
        Assert.NotNull(productTagEntity);
        Assert.Equal(productTag.TagName, productTagEntity.TagName);
        Assert.Equal(productTag.TagValue, productTagEntity.TagValue);
    }

    [Fact]
    public void Should_Map_ProductTag_Collection()
    {
        // Arrange
        var productTags = new List<ProductTag>
        {
            new() { TagName = "Category", TagValue = "Electronics" },
            new() { TagName = "Brand", TagValue = "Apple" },
            new() { TagName = "Color", TagValue = "Silver" }
        };

        // Act
        var productTagEntities = _mapper.Map<List<ProductTagEntity>>(productTags);

        // Assert
        Assert.NotNull(productTagEntities);
        Assert.Equal(3, productTagEntities.Count);

        for (int i = 0; i < productTags.Count; i++)
        {
            Assert.Equal(productTags[i].TagName, productTagEntities[i].TagName);
            Assert.Equal(productTags[i].TagValue, productTagEntities[i].TagValue);
        }
    }

    [Fact]
    public void Should_Map_Product_With_ProductTags_Collection()
    {
        // Arrange
        var productModel = new ProductModel
        {
            ProductName = "iPhone 15",
            Price = 999.99m,
            ProductTags = new List<ProductTag>
            {
                new() { TagName = "Category", TagValue = "Smartphone" },
                new() { TagName = "Brand", TagValue = "Apple" },
                new() { TagName = "Storage", TagValue = "128GB" },
                new() { TagName = "Color", TagValue = "Blue" }
            }
        };

        // Act
        var productEntity = _mapper.MapSingleObject<ProductModel, ProductEntity>(productModel);

        // Assert
        Assert.NotNull(productEntity);
        Assert.Equal(productModel.ProductName, productEntity.ProductName);
        Assert.Equal(productModel.Price, productEntity.Price);

        // Verify ProductTags collection mapping
        Assert.NotNull(productEntity.ProductTags);
        Assert.Equal(4, productEntity.ProductTags.Count);

        Assert.Equal("Category", productEntity.ProductTags[0].TagName);
        Assert.Equal("Smartphone", productEntity.ProductTags[0].TagValue);

        Assert.Equal("Brand", productEntity.ProductTags[1].TagName);
        Assert.Equal("Apple", productEntity.ProductTags[1].TagValue);

        Assert.Equal("Storage", productEntity.ProductTags[2].TagName);
        Assert.Equal("128GB", productEntity.ProductTags[2].TagValue);

        Assert.Equal("Color", productEntity.ProductTags[3].TagName);
        Assert.Equal("Blue", productEntity.ProductTags[3].TagValue);
    }

    [Fact]
    public void Should_Map_User_With_Products_And_ProductTags_Deep_Nesting()
    {
        // Arrange
        var userModel = new UserModel
        {
            Name = "Tech Enthusiast",
            Age = 32,
            Addresses = new AddressModel
            {
                Street = "123 Tech Street",
                City = "Silicon Valley"
            },
            Products = new List<ProductModel>
            {
                new()
                {
                    ProductName = "MacBook Pro",
                    Price = 2499.99m,
                    ProductTags = new List<ProductTag>
                    {
                        new() { TagName = "Category", TagValue = "Laptop" },
                        new() { TagName = "Brand", TagValue = "Apple" },
                        new() { TagName = "Screen", TagValue = "16-inch" }
                    }
                },
                new()
                {
                    ProductName = "AirPods Pro",
                    Price = 249.99m,
                    ProductTags = new List<ProductTag>
                    {
                        new() { TagName = "Category", TagValue = "Audio" },
                        new() { TagName = "Brand", TagValue = "Apple" },
                        new() { TagName = "Feature", TagValue = "Noise Cancelling" }
                    }
                }
            }
        };

        // Act
        var userEntity = _mapper.MapSingleObject<UserModel, UserEntity>(userModel);

        // Assert
        Assert.NotNull(userEntity);
        Assert.Equal("Tech Enthusiast", userEntity.Name);
        Assert.Equal(32, userEntity.Age);

        // Verify address
        Assert.NotNull(userEntity.Addresses);
        Assert.Equal("123 Tech Street", userEntity.Addresses.Street);
        Assert.Equal("Silicon Valley", userEntity.Addresses.City);

        // Verify products with nested tags
        Assert.NotNull(userEntity.Products);
        Assert.Equal(2, userEntity.Products.Count);

        // Verify first product and its tags
        var macbook = userEntity.Products[0];
        Assert.Equal("MacBook Pro", macbook.ProductName);
        Assert.Equal(2499.99m, macbook.Price);
        Assert.NotNull(macbook.ProductTags);
        Assert.Equal(3, macbook.ProductTags.Count);
        Assert.Equal("Category", macbook.ProductTags[0].TagName);
        Assert.Equal("Laptop", macbook.ProductTags[0].TagValue);

        // Verify second product and its tags
        var airpods = userEntity.Products[1];
        Assert.Equal("AirPods Pro", airpods.ProductName);
        Assert.Equal(249.99m, airpods.Price);
        Assert.NotNull(airpods.ProductTags);
        Assert.Equal(3, airpods.ProductTags.Count);
        Assert.Equal("Audio", airpods.ProductTags.Where(t => t.TagName == "Category").First().TagValue);
        Assert.Equal("Noise Cancelling", airpods.ProductTags.Where(t => t.TagName == "Feature").First().TagValue);
    }

    [Fact]
    public void Should_Handle_Empty_ProductTags_Collection()
    {
        // Arrange
        var productModel = new ProductModel
        {
            ProductName = "Simple Product",
            Price = 19.99m,
            ProductTags = new List<ProductTag>() // Empty collection
        };

        // Act
        var productEntity = _mapper.MapSingleObject<ProductModel, ProductEntity>(productModel);

        // Assert
        Assert.NotNull(productEntity);
        Assert.Equal("Simple Product", productEntity.ProductName);
        Assert.Equal(19.99m, productEntity.Price);
        Assert.NotNull(productEntity.ProductTags);
        Assert.Empty(productEntity.ProductTags);
    }

    [Fact]
    public void Should_Handle_Null_ProductTags_Collection()
    {
        // Arrange
        var productModel = new ProductModel
        {
            ProductName = "Basic Product",
            Price = 9.99m,
            ProductTags = null // Null collection
        };

        // Act
        var productEntity = _mapper.MapSingleObject<ProductModel, ProductEntity>(productModel);

        // Assert
        Assert.NotNull(productEntity);
        Assert.Equal("Basic Product", productEntity.ProductName);
        Assert.Equal(9.99m, productEntity.Price);
        Assert.Null(productEntity.ProductTags);
    }

    [Fact]
    public void Should_Map_Reverse_ProductTag_Entity_To_Model()
    {
        // Arrange
        var productTagEntity = new ProductTagEntity
        {
            TagName = "Quality",
            TagValue = "Premium"
        };

        // Act
        var productTag = _mapper.MapSingleObject<ProductTagEntity, ProductTag>(productTagEntity);

        // Assert
        Assert.NotNull(productTag);
        Assert.Equal(productTagEntity.TagName, productTag.TagName);
    }
}