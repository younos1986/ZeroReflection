using ZeroReflection.Mediator;

namespace Application.Features.Products;

public class AddProductCommand: IRequest<Unit>
{
    
}

public class AddProductCommandHandler: IRequestHandler<AddProductCommand, Unit>
{
    public async Task<Unit> Handle(AddProductCommand request, CancellationToken cancellationToken)
    {
        // Logic to add a product
        Console.WriteLine("Product added successfully.");
        return Unit.Value;
    }
}