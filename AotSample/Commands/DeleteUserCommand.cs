using ZeroReflection.Mediator;

namespace AotSample.Commands;

public class DeleteUserCommand : IRequest<Unit>
{
    public required string Id { get; set; }
}

public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, Unit>
{
    public Task<Unit> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Deleted user {request.Id}");
        return Task.FromResult(Unit.Value);
    }
}
