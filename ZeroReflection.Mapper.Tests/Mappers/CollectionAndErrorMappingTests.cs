using Microsoft.Extensions.DependencyInjection;
using ZeroReflection.Mapper.Generated;
using ZeroReflection.Mapper.Tests.Models.Entities;

namespace ZeroReflection.Mapper.Tests.Mappers;

public class CollectionAndErrorMappingTests
{
    private readonly IMapper _mapper;

    public CollectionAndErrorMappingTests()
    {
        var services = new ServiceCollection();
        services.RegisterZeroReflectionMapping();
        var serviceProvider = services.BuildServiceProvider();
        _mapper = serviceProvider.GetRequiredService<IMapper>();
    }

    [Fact]
    public void Should_Throw_When_Mapping_Array_To_List()
    {
        // Arrange
        TestModel[] models =
        [
            new() { Name = "Model 1", Age = 20 },
            new() { Name = "Model 2", Age = 30 }
        ];

        // Act + Assert
        Assert.Throws<InvalidOperationException>(() => _mapper.Map<List<TestEntity>>(models));
    }

    [Fact]
    public void Should_Map_Array_To_Array_Correctly()
    {
        // Arrange
        TestModel[] models =
        [
            new() { Name = "A", Age = 1 },
            new() { Name = "B", Age = 2 },
            new() { Name = "C", Age = 3 }
        ];

        // Act
        var entities = _mapper.Map<TestEntity[]>(models);

        // Assert
        Assert.NotNull(entities);
        Assert.Equal(3, entities.Length);
        Assert.Equal("A", entities[0].Name);
        Assert.Equal("B", entities[1].Name);
        Assert.Equal("C", entities[2].Name);
    }

    [Fact]
    public void Should_Throw_When_Mapping_List_To_Array()
    {
        // Arrange
        var models = new List<TestModel>
        {
            new() { Name = "X", Age = 10 },
            new() { Name = "Y", Age = 11 }
        };

        // Act + Assert
        Assert.Throws<InvalidOperationException>(() => _mapper.Map<TestEntity[]>(models));
    }

    [Fact]
    public void Should_Return_Empty_Array_When_Source_Array_Empty()
    {
        // Arrange
        TestModel[] models = Array.Empty<TestModel>();

        // Act
        var entities = _mapper.Map<TestEntity[]>(models);

        // Assert
        Assert.NotNull(entities);
        Assert.Empty(entities);
    }

    [Fact]
    public void Should_Support_Typed_Generic_Map_For_Collections()
    {
        // Arrange
        var models = new List<TestModel>
        {
            new() { Name = "John", Age = 40 }
        };

        // Act
        var entities = _mapper.Map<List<TestModel>, List<TestEntity>>(models);

        // Assert
        Assert.Single(entities);
        Assert.Equal("John", entities[0].Name);
        Assert.Equal(40, entities[0].Age);
    }

    [Fact]
    public void MapSingleObject_Should_Return_Default_When_Source_Is_Null()
    {
        // Arrange
        TestModel? model = null;

        // Act
        var entity = _mapper.MapSingleObject<TestModel, TestEntity>(model!);

        // Assert
        Assert.Null(entity);
    }

    [Fact]
    public void Should_Throw_For_Unconfigured_Mapping()
    {
        // Arrange + Act + Assert
        Assert.Throws<InvalidOperationException>(() => _mapper.Map<int>("123"));
    }
}
