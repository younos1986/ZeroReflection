using AotSample.Models.ViewModels;
using ZeroReflection.Mediator;

namespace AotSample.Commands;

public class ListUsersQuery : IRequest<List<UserModel>>
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class ListUsersQueryHandler : IRequestHandler<ListUsersQuery, List<UserModel>>
{
    public Task<List<UserModel>> Handle(ListUsersQuery request, CancellationToken cancellationToken)
    {
        // Mock implementation
        var users = new List<UserModel>();
        for (int i = 0; i < request.PageSize; i++)
        {
            users.Add(new UserModel
            {
                Name = $"User {i + (request.Page - 1) * request.PageSize}",
                Age = 20 + i,
                Email = $"user{i}@example.com"
            });
        }
        return Task.FromResult(users);
    }
}
