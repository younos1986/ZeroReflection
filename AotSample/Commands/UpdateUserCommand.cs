using AotSample.Models.ViewModels;
using ZeroReflection.Mediator;

namespace AotSample.Commands;

public class UpdateUserCommand : IRequest<Unit>
{
    public string Id { get; set; } = string.Empty;
    public required UserModel UserModel { get; set; }
}

public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, Unit>
{
    public Task<Unit> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Updated user {request.Id}: {request.UserModel.Name}");
        return Task.FromResult(Unit.Value);
    }
}
