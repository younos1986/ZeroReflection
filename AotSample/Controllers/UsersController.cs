using AotSample.Commands;
using AotSample.Models.ViewModels;
using ZeroReflection.Api;
using ZeroReflection.Mediator;

namespace AotSample.Controllers;

[ApiController("api/users")]
public class UsersController
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost(Summary = "Create a new user")]
    public async Task<Unit> CreateUser([FromBody] CreateUserCommand command, CancellationToken ct)
    {
        return await _mediator.Send<Unit>(command, ct);
    }

    [HttpGet("{id}", Summary = "Get user by ID")]
    public async Task<UserModel> GetUser([FromRoute] string id, CancellationToken ct)
    {
        return await _mediator.Send(new GetUserQuery { Id = id }, ct);
    }

    [HttpGet(Summary = "List all users")]
    public async Task<List<UserModel>> ListUsers([FromQuery] int page, [FromQuery] int pageSize, CancellationToken ct)
    {
        return await _mediator.Send(new ListUsersQuery { Page = page, PageSize = pageSize }, ct);
    }

    [HttpPut("{id}", Summary = "Update user")]
    public async Task<Unit> UpdateUser([FromRoute] string id, [FromBody] UpdateUserCommand command, CancellationToken ct)
    {
        command.Id = id;
        return await _mediator.Send<Unit>(command, ct);
    }

    [HttpDelete("{id}", Summary = "Delete user")]
    public async Task<Unit> DeleteUser([FromRoute] string id, CancellationToken ct)
    {
        return await _mediator.Send(new DeleteUserCommand { Id = id }, ct);
    }
}
