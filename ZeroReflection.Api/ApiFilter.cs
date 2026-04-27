using System.Collections.Generic;
using System.Threading.Tasks;

namespace ZeroReflection.Api;

public class ApiFilterContext
{
    public string Path { get; init; } = string.Empty;
    public string Method { get; init; } = string.Empty;
    public Dictionary<string, string> QueryValues { get; init; } = new();
    public string? Body { get; init; }
}

public abstract class ApiFilter
{
    public virtual Task StartRequestAsync(ApiFilterContext context) => Task.CompletedTask;
    public virtual Task StartResponseAsync(ApiFilterContext context, RouteResult result) => Task.CompletedTask;
}
