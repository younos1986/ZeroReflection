using System.Threading;
using System.Threading.Tasks;

namespace ZeroReflection.Mediator;

public interface IMediator
{
    Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);
}