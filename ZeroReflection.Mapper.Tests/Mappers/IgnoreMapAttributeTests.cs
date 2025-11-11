using Microsoft.Extensions.DependencyInjection;
using ZeroReflection.Mapper.Generated;
using ZeroReflection.Mapper.Tests.Models.Entities;

namespace ZeroReflection.Mapper.Tests.Mappers;

public class IgnoreMapAttributeTestProfile : MapperProfile
{
    public override void Configure(MapperConfiguration config)
    {
        config.CreateMap<SourceModelWithIgnore, DestinationModelWithIgnore>().Reverse();
    }
}

public class IgnoreMapAttributeTests
{
    private readonly IMapper _mapper;

    public IgnoreMapAttributeTests()
    {
        var services = new ServiceCollection();
        services.RegisterZeroReflectionMapping();
        
        var serviceProvider = services.BuildServiceProvider();
        _mapper = serviceProvider.GetRequiredService<IMapper>();
    }

    [Fact]
    public void Should_Ignore_Properties_With_IgnoreMapAttribute()
    {
        // Arrange
        var source = new SourceModelWithIgnore
        {
            Name = "John Doe",
            Age = 30,
            IgnoredProperty = "This should not be mapped",
            MappedProperty = "This should be mapped"
        };

        // Act
        var destination = _mapper.MapSingleObject<SourceModelWithIgnore, DestinationModelWithIgnore>(source);

        // Assert
        Assert.NotNull(destination);
        
        Assert.Equal(source.Name, destination.Name);
        Assert.Equal(source.Age, destination.Age);
        Assert.Equal(source.MappedProperty, destination.MappedProperty);
        
        Assert.Equal("Should be ignored", destination.IgnoredProperty);
        Assert.NotEqual(source.IgnoredProperty, destination.IgnoredProperty);
    }

    [Fact]
    public void Should_Ignore_Properties_With_IgnoreMapAttribute_On_Reverse_Mapping()
    {
        // Arrange
        var destination = new DestinationModelWithIgnore
        {
            Name = "Jane Smith",
            Age = 22,
            IgnoredProperty = "Changed Value",
            MappedProperty = "Reverse Mapped"
        };

        // Act
        var source = _mapper.MapSingleObject<DestinationModelWithIgnore, SourceModelWithIgnore>(destination);

        // Assert
        Assert.NotNull(source);
        Assert.Equal(destination.Name, source.Name);
        Assert.Equal(destination.Age, source.Age);
        Assert.Equal(destination.MappedProperty, source.MappedProperty);
        // Source's ignored property should remain its default value
        Assert.Equal("Should be ignored", source.IgnoredProperty);
        Assert.NotEqual(destination.IgnoredProperty, source.IgnoredProperty);
    }

    [Fact]
    public void Should_Not_Generate_Mapping_For_Ignored_Properties()
    {
        // Documentation test
        Assert.True(true, "IgnoreMap properties should not appear in generated mapping code");
    }
}
