using Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private static readonly List<User> Users =
    [
        new() { Id = 1, Name = "Alice", Email = "alice@example.com" },
        new() { Id = 2, Name = "Bob", Email = "bob@example.com" }
    ];

    [HttpGet]
    public List<User> GetAll() => Users;

    [HttpGet("{id}")]
    public User? Get([FromRoute] string id)
    {
        return Users.FirstOrDefault(u => u.Id == int.Parse(id));
    }

    [HttpPost]
    public User Create([FromBody] User user)
    {
        user.Id = Users.Count > 0 ? Users.Max(u => u.Id) + 1 : 1;
        Users.Add(user);
        return user;
    }

    [HttpPut("{id}")]
    public User? Update([FromRoute] string id, [FromBody] User user)
    {
        var existing = Users.FirstOrDefault(u => u.Id == int.Parse(id));
        if (existing is null) return null;
        existing.Name = user.Name;
        existing.Email = user.Email;
        return existing;
    }

    [HttpDelete("{id}")]
    public bool Delete([FromRoute] string id)
    {
        var user = Users.FirstOrDefault(u => u.Id == int.Parse(id));
        if (user is null) return false;
        Users.Remove(user);
        return true;
    }
}
