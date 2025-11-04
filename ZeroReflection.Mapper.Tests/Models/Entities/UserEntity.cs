namespace ZeroReflection.Mapper.Tests.Models.Entities;

public class UserEntity
{
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
    public AddressEntity Addresses { get; set; } = new();
    public List<ProductEntity> Products { get; set; } = new();
}
