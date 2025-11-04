using Microsoft.Extensions.DependencyInjection;
using ZeroReflection.Mapper;
using ZeroReflection.Mapper.Generated;
using ZeroReflection.Mapper.Tests.Models.DTOs;
using ZeroReflection.Mapper.Tests.Models.Entities;

namespace ZeroReflection.Mapper.Tests.CustomMappers;

// User-specific mapping profile
public class UserMappingProfile : MapperProfile
{
    public override void Configure(MapperConfiguration config)
    {
        // Strongly typed mapping with custom property mappings
        config.CreateMap<CustomTestUser, CustomTestUserDto>()
            .ForMember(dest => dest.Name, source => $"{source.FirstName} {source.LastName}")
            .ForMember(dest => dest.PK, source => $"USER#{source.Id}")
            .ForMember(dest => dest.SK, source => $"USER#{source.Id}")
            .Reverse();
    }

    // Custom property mapping using attributes
    [CustomPropertyMapping(typeof(CustomTestUser), typeof(CustomTestUserDto), "Age")]
    private int CalculateAge(CustomTestUser user)
    {
        return DateTime.Now.Year - user.BirthDate.Year;
    }
}

public class UserModelTests
{
    private readonly IMapper _mapper;

    public UserModelTests()
    {
        // Arrange
        var services = new ServiceCollection();
        services.RegisterZeroReflectionMapping();
        
        var serviceProvider = services.BuildServiceProvider();
        _mapper = serviceProvider.GetRequiredService<IMapper>();
    }

    [Fact]
    public void Should_Map_User_To_UserDto_With_Custom_Properties()
    {
        // Arrange
        var user = new CustomTestUser
        {
            Id = 123,
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            BirthDate = new DateTime(1990, 5, 15)
        };

        // Act
        var userDto = _mapper.MapSingleObject<CustomTestUser, CustomTestUserDto>(user);

        // Assert
        Assert.NotNull(userDto);
        Assert.Equal(123, userDto.Id);
        Assert.Equal("John Doe", userDto.Name); // Custom ForMember mapping
        Assert.Equal("john.doe@example.com", userDto.Email);
        Assert.Equal("USER#123", userDto.PK); // Custom ForMember mapping
        Assert.Equal("USER#123", userDto.SK); // Custom ForMember mapping
        Assert.Equal(DateTime.Now.Year - 1990, userDto.Age); // Custom property mapping via attribute
    }

    [Fact]
    public void Should_Map_User_With_Null_Values()
    {
        // Arrange
        var user = new CustomTestUser
        {
            Id = 456,
            FirstName = "",
            LastName = "",
            Email = "test@example.com",
            BirthDate = new DateTime(1985, 1, 1)
        };

        // Act
        var userDto = _mapper.MapSingleObject<CustomTestUser, CustomTestUserDto>(user);

        // Assert
        Assert.NotNull(userDto);
        Assert.Equal(456, userDto.Id);
        Assert.Equal(" ", userDto.Name); // Empty first and last name with space
        Assert.Equal("USER#456", userDto.PK);
        Assert.Equal("USER#456", userDto.SK);
        Assert.Equal(DateTime.Now.Year - 1985, userDto.Age);
    }

    [Fact]
    public void Should_Calculate_Age_Correctly()
    {
        // Arrange
        var user = new CustomTestUser
        {
            Id = 789,
            FirstName = "Jane",
            LastName = "Smith",
            Email = "jane@example.com",
            BirthDate = new DateTime(2000, 12, 25) // Born in 2000
        };

        // Act
        var userDto = _mapper.MapSingleObject<CustomTestUser, CustomTestUserDto>(user);

        // Assert
        Assert.Equal(DateTime.Now.Year - 2000, userDto.Age);
    }
}
