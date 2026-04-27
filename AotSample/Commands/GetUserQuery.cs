using AotSample.Models.ViewModels;
using ZeroReflection.Mediator;

namespace AotSample.Commands;

public class GetUserQuery : IRequest<UserModel>
{
    public required string Id { get; set; }
}

public class GetUserQueryHandler : IRequestHandler<GetUserQuery, UserModel>
{
    public Task<UserModel> Handle(GetUserQuery request, CancellationToken cancellationToken)
    {
        // Mock implementation - in real scenario, fetch from database
        return Task.FromResult(new UserModel
        {
            Name = "John Doe",
            Age = 30,
            Email = $"user{request.Id}@example.com"
        });
    }
}
