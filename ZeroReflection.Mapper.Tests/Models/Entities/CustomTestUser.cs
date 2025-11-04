namespace ZeroReflection.Mapper.Tests.Models.Entities;

public class CustomTestUser
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime BirthDate { get; set; }
    public string Email { get; set; } = string.Empty;
    public List<CustomTestOrder> Orders { get; set; } = new();
}
