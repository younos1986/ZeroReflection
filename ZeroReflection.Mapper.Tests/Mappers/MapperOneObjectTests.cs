using Microsoft.Extensions.DependencyInjection;
using ZeroReflection.Mapper.Generated;
using ZeroReflection.Mapper.Tests.Models.Entities;

namespace ZeroReflection.Mapper.Tests.Mappers;


public class TestProfile : MapperProfile
{
    public override void Configure(MapperConfiguration config)
    {
        config.CreateMap<TestModel, TestEntity>().Reverse();
    }
}

public class MapperOneObjectTests
{
    private readonly IMapper _mapper;

    public MapperOneObjectTests()
    {
        // Arrange
        var services = new ServiceCollection();
        services.RegisterZeroReflectionMapping();
        
        var serviceProvider = services.BuildServiceProvider();
        _mapper = serviceProvider.GetRequiredService<IMapper>();
    }
    
    [Fact]
    public void Should_Map_Model_To_Entity()
    {
        var testModel = new TestModel
        {
            Name = "John Doe",
            Age = 30,
            InstaPageId = "test123"
        };
        
        // Act
        var testEntity = _mapper.MapSingleObject<TestModel, TestEntity>(testModel);
        
        // Assert
        Assert.NotNull(testEntity);
        Assert.Equal(testModel.Name, testEntity.Name);
        Assert.Equal(testModel.Age, testEntity.Age);
        Assert.Equal(testModel.InstaPageId, testEntity.InstaPageId);
    }
    
    [Fact]
    public void Should_Map_Entity_To_Model()
    {
        var testEntity = new TestEntity
        {
            Name = "Jane Smith",
            Age = 25,
            InstaPageId = "entity456"
        };
        
        // Act
        var testModel = _mapper.MapSingleObject<TestEntity, TestModel>(testEntity);
        
        // Assert
        Assert.NotNull(testModel);
        Assert.Equal(testEntity.Name, testModel.Name);
        Assert.Equal(testEntity.Age, testModel.Age);
        Assert.Equal(testEntity.InstaPageId, testModel.InstaPageId);
    }
    
    [Fact]
    public void Should_Map_Collection_Of_Models_To_Entities()
    {
        var testModels = new List<TestModel>
        {
            new() { Name = "John Doe", Age = 30 },
            new() { Name = "Jane Smith", Age = 25 },
            new() { Name = "Bob Johnson", Age = 35 }
        };
        
        // Act
        var testEntities = _mapper.Map<List<TestEntity>>(testModels);
        
        // Assert
        Assert.NotNull(testEntities);
        Assert.Equal(3, testEntities.Count);
        
        for (int i = 0; i < testModels.Count; i++)
        {
            Assert.Equal(testModels[i].Name, testEntities[i].Name);
            Assert.Equal(testModels[i].Age, testEntities[i].Age);
        }
    }
    
    [Fact]
    public void Should_Handle_Null_Source_Object()
    {
        TestModel? nullModel = null;
        
        // Act
        var result = _mapper.Map<TestEntity>(nullModel!);
        
        // Assert
        Assert.Null(result);
    }
    
    [Fact]
    public void Should_Map_List_To_List()
    {
        var testModels = new List<TestModel>
        {
            new() { Name = "Model 1", Age = 20 },
            new() { Name = "Model 2", Age = 30 }
        };
        
        // Act
        var testEntityList = _mapper.Map<List<TestEntity>>(testModels);
        
        // Assert
        Assert.NotNull(testEntityList);
        Assert.Equal(2, testEntityList.Count);
        Assert.Equal("Model 1", testEntityList[0].Name);
        Assert.Equal("Model 2", testEntityList[1].Name);
    }
    
    [Fact]
    public void Should_Handle_Null_InstaPageId_With_Default_Value()
    {
        var testModel = new TestModel
        {
            Name = "John Doe",
            Age = 30,
            InstaPageId = null // Nullable source
        };
        
        // Act
        var testEntity = _mapper.MapSingleObject<TestModel, TestEntity>(testModel);
        
        // Assert
        Assert.NotNull(testEntity);
        Assert.Equal(testModel.Name, testEntity.Name);
        Assert.Equal(testModel.Age, testEntity.Age);
        Assert.Equal(string.Empty, testEntity.InstaPageId); // Should use default value for non-nullable string
    }
}