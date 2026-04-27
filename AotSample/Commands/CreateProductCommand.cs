using ZeroReflection.Mapper;
using ZeroReflection.Mediator;

namespace AotSample.Commands;

public class CreateProductCommand : IRequest<Unit>
{
    public required ProductModel ProductModel { get; set; }
}

public class CreateProductCommandValidator : IValidator<CreateProductCommand>
{
    public void Validate(CreateProductCommand request)
    {
        if (request.ProductModel is null)
            throw new ArgumentNullException(nameof(request.ProductModel));
    }
}

public class CreateProductCommandHandler(IMapper mapper) : IRequestHandler<CreateProductCommand, Unit>
{
    public Task<Unit> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var personEntity = mapper.MapSingleObject<ProductModel, ProductEntity>(request.ProductModel);

        Console.WriteLine(personEntity.Name + " " + personEntity.Age + " " + personEntity.Email);

        return Task.FromResult(Unit.Value);
    }
}
