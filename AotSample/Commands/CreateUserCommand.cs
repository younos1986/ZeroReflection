using AotSample.Models.Entities;
using AotSample.Models.ViewModels;
using ZeroReflection.Mapper;
using ZeroReflection.Mediator;

namespace AotSample.Commands;

public class CreateUserCommand: IRequest<Unit>
{
    public required UserModel UserModel { get; set; }
}

public class CreateUserCommandValidator : IValidator<CreateUserCommand>
{
    public void Validate(CreateUserCommand request)
    {
        if (request.UserModel is null)
            throw new ArgumentNullException(nameof(request.UserModel));
    }
}

public class CreateUserCommandHandler(IMapper mapper) : IRequestHandler<CreateUserCommand, Unit>
{
    public Task<Unit> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var personEntity = mapper.MapSingleObject<UserModel, UserEntity>(request.UserModel);

        Console.WriteLine(personEntity.Name + " " + personEntity.Age + " " + personEntity.Email);
        
        return Task.FromResult(Unit.Value);
    }
}
