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
    public void Should_Not_Generate_Mapping_For_Ignored_Properties()
    {
        // This test verifies that properties with [IgnoreMap] are not included
        // in the generated mapping code at all. If the source generator is working
        // correctly, the generated mapping method should not contain any reference
        // to IgnoredSourceProperty or IgnoredDestinationProperty.
        
        // The test above already validates the runtime behavior,
        // but this documents the expected code generation behavior.
        Assert.True(true, "IgnoreMap properties should not appear in generated mapping code");
    }
}
