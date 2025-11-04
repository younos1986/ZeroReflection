namespace ZeroReflection.Mapper.Tests.Models.Entities;

public class DestinationModelWithIgnore
{
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
    public string IgnoredProperty { get; set; } = "Should be ignored";
    public string MappedProperty { get; set; } = string.Empty;
}
