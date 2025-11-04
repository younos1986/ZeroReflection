using Microsoft.Extensions.DependencyInjection;
using ZeroReflection.Mapper.Generated;

namespace ZeroReflection.Mapper.Tests.Mappers;

public class UnmappablePropertySource
{
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
    
    [IgnoreMap]
    public string IgnoredSourceProperty { get; set; } = string.Empty;
    
    public DateTime CreatedDate { get; set; }
    public bool IsActive { get; set; }
}

public class UnmappablePropertyDestination
{
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
    
    // This property has no matching source property
    public string MissingInSource { get; set; } = string.Empty;
    
    // This property has [IgnoreMap] attribute
    [IgnoreMap]
    public string IgnoredDestProperty { get; set; } = string.Empty;
    
    // This property has type mismatch (source is DateTime, this is string)
    public string CreatedDate { get; set; } = string.Empty;
    
    // This property points to ignored source property via MapTo
    [MapTo("IgnoredSourceProperty")]
    public string MappedToIgnored { get; set; } = string.Empty;
    
    // This property points to non-existent source property
    [MapTo("NonExistentProperty")]
    public string MappedToNonExistent { get; set; } = string.Empty;
    
    // This property has incompatible type via MapTo
    [MapTo("IsActive")]
    public string IncompatibleMapTo { get; set; } = string.Empty;
}

public class UnmappableTestProfile : MapperProfile
{
    public override void Configure(MapperConfiguration config)
    {
        config.CreateMap<UnmappablePropertySource, UnmappablePropertyDestination>();
    }
}

public class UnmappablePropertyTests
{
    private readonly IMapper _mapper;

    public UnmappablePropertyTests()
    {
        var services = new ServiceCollection();
        services.RegisterZeroReflectionMapping();
        var serviceProvider = services.BuildServiceProvider();
        _mapper = serviceProvider.GetRequiredService<IMapper>();
    }
    
    [Fact]
    public void Should_Map_Available_Properties_And_Document_Unmappable_Ones()
    {
        var source = new UnmappablePropertySource
        {
            Name = "Test User",
            Age = 30,
            IgnoredSourceProperty = "This should be ignored",
            CreatedDate = DateTime.Now,
            IsActive = true
        };
        
        // Act
        var destination = _mapper.MapSingleObject<UnmappablePropertySource, UnmappablePropertyDestination>(source);
        
        // Assert - verify mappable properties are mapped correctly
        Assert.NotNull(destination);
        Assert.Equal(source.Name, destination.Name);
        Assert.Equal(source.Age, destination.Age);
        
        // Unmappable properties should use their default values
        Assert.Equal(string.Empty, destination.MissingInSource);
        Assert.Equal(string.Empty, destination.IgnoredDestProperty);
        Assert.Equal(string.Empty, destination.CreatedDate);
        Assert.Equal(string.Empty, destination.MappedToIgnored);
        Assert.Equal(string.Empty, destination.MappedToNonExistent);
        Assert.Equal(string.Empty, destination.IncompatibleMapTo);
    }
}
