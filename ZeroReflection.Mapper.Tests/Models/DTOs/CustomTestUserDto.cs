namespace ZeroReflection.Mapper.Tests.Models.DTOs;

public class CustomTestUserDto
{
    public required int Id { get; set; }
    public required string Name { get; set; }
    public required string Email { get; set; }
    public required string PK { get; set; }
    public required string SK { get; set; }
    public int Age { get; set; }
}
