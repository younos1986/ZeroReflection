using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace ZeroReflection.Mediator;

public class MediatorImplementation : IMediator
{
    private readonly IGeneratedMediatorDispatcher _dispatcher;

    public MediatorImplementation(IGeneratedMediatorDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        var requestType = request.GetType();
        var responseType = typeof(TResponse);

        // Validation logic
        _dispatcher.TryValidate(request, requestType);

        // Handle request
        var (handled, result) = await _dispatcher.TryHandle(request, requestType, responseType, cancellationToken);
        
        if (!handled)
            throw new InvalidOperationException($"No handler registered for {requestType.FullName} with response type {responseType.FullName}");

        return (TResponse)result;
    }
}