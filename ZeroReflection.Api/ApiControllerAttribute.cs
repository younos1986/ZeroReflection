using System;

namespace ZeroReflection.Api;

[AttributeUsage(AttributeTargets.Class)]
public class ApiControllerAttribute : Attribute
{
    public string Route { get; }
    
    public ApiControllerAttribute(string route)
    {
        Route = route ?? throw new ArgumentNullException(nameof(route));
    }
}
