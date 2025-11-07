using Microsoft.Extensions.DependencyInjection;
using ZeroReflection.Mapper.Generated;

namespace ZeroReflection.Mapper.Tests.Mappers;

public class ThrowMissingSource
{
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
}

public class ThrowMissingDestination
{
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
    
    // This property has no matching source property
    public string MissingProperty { get; set; } = string.Empty;
}

public class ThrowMissingEnabledProfile : MapperProfile
{
    public override void Configure(MapperConfiguration config)
    {
        config.ThrowIfPropertyMissing = true;
        config.CreateMap<ThrowMissingSource, ThrowMissingDestination>();
    }
}

public class ThrowMissingDisabledProfile : MapperProfile
{
    public override void Configure(MapperConfiguration config)
    {
        config.ThrowIfPropertyMissing = false;
        config.CreateMap<ThrowMissingSource, ThrowMissingDestination>();
    }
}

public class ThrowIfPropertyMissingTests
{
    [Fact]
    public void Should_Throw_When_Property_Missing_And_Flag_Enabled()
    {
        // This test verifies that when ThrowIfPropertyMissing = true,
        // the generated mapper throws an exception for unmappable properties
        
        var source = new ThrowMissingSource
        {
            Name = "Test User",
            Age = 30
        };
        
        // The generated code should throw an InvalidOperationException
        // because MissingProperty cannot be mapped from source
        var exception = Assert.Throws<InvalidOperationException>(() =>
        {
            // This would be the generated extension method call
            source.MapToThrowMissingDestination();
        });
        
        Assert.Contains("Property mapping missing", exception.Message);
        Assert.Contains("MissingProperty", exception.Message);
        Assert.Contains("Fix the model or add explicit mapping configuration", exception.Message);
    }
    
    [Fact]
    public void Should_Not_Throw_When_Property_Missing_And_Flag_Disabled()
    {
        // This test verifies that when ThrowIfPropertyMissing = false,
        // the generated mapper does not throw and uses default values
        
        var source = new ThrowMissingSource
        {
            Name = "Test User",
            Age = 30
        };
        
        // Should not throw when flag is disabled
        var destination = source.MapToThrowMissingDestination();
        
        Assert.NotNull(destination);
        Assert.Equal(source.Name, destination.Name);
        Assert.Equal(source.Age, destination.Age);
        Assert.Equal(string.Empty, destination.MissingProperty); // Default value
    }
}