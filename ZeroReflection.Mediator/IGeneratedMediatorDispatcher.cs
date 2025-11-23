using System.Threading;
using System.Threading.Tasks;

namespace ZeroReflection.Mediator;

public interface IGeneratedMediatorDispatcher
{
    Task<(bool handled, object result)> TryHandle(object request, System.Type requestType, System.Type responseType, CancellationToken cancellationToken);
    bool TryValidate(object request, System.Type requestType);
}

public sealed class NullGeneratedMediatorDispatcher : IGeneratedMediatorDispatcher
{
    public Task<(bool handled, object result)> TryHandle(object request, System.Type requestType, System.Type responseType, CancellationToken cancellationToken)
    {
        return Task.FromResult<(bool, object)>((false, null!));
    }

    public bool TryValidate(object request, System.Type requestType)
    {
        return false;
    }
}
