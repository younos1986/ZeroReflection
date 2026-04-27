using AotSample.Commands;
using ZeroReflection.Api;
using ZeroReflection.Mediator;

namespace AotSample.Controllers;

[ApiController("api/products")]
public class ProductController
{
    private readonly IMediator _mediator;

    public ProductController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost(Summary = "Create a new product")]
    public async Task<Unit> CreateProduct([FromBody] CreateProductCommand command, CancellationToken ct)
    {
        return await _mediator.Send<Unit>(command, ct);
    }

}
