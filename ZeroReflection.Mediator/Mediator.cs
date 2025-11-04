using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ZeroReflection.Mediator;

public class MediatorImplementation(IServiceProvider serviceProvider) : IMediator
{
    public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        // Validation logic
        var validatorType = typeof(IValidator<>).MakeGenericType(request.GetType());
        var validator = serviceProvider.GetService(validatorType);
        if (validator != null)
        {
            ((dynamic)validator).Validate((dynamic)request);
        }

        var handlerType = typeof(IRequestHandler<,>).MakeGenericType(request.GetType(), typeof(TResponse));
        dynamic handler = serviceProvider.GetService(handlerType);
        if (handler == null)
            throw new InvalidOperationException($"No handler registered for {request.GetType()}");
        return await handler.Handle((dynamic)request, cancellationToken);
    }
}