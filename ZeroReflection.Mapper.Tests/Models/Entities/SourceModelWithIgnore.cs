using ZeroReflection.Mapper;

namespace ZeroReflection.Mapper.Tests.Models.Entities;

public class SourceModelWithIgnore
{
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
    
    [IgnoreMap]
    public string IgnoredProperty { get; set; } = "Should be ignored";
    
    public string MappedProperty { get; set; } = "Should be mapped";
}
