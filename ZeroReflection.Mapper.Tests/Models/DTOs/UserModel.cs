namespace ZeroReflection.Mapper.Tests.Models.DTOs;

public class UserModel
{
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
    public AddressModel Addresses { get; set; } = new();
    public List<ProductModel> Products { get; set; } = new();
}
