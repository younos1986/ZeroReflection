using System;

namespace ZeroReflection.Api;

public abstract class HttpMethodAttribute : Attribute
{
    public string? Route { get; }
    public string? Summary { get; set; }
    public string? Description { get; set; }
    public string[]? Tags { get; set; }
    
    protected HttpMethodAttribute(string? route = null)
    {
        Route = route;
    }
}

[AttributeUsage(AttributeTargets.Method)]
public class HttpGetAttribute : HttpMethodAttribute
{
    public HttpGetAttribute(string? route = null) : base(route) { }
}

[AttributeUsage(AttributeTargets.Method)]
public class HttpPostAttribute : HttpMethodAttribute
{
    public HttpPostAttribute(string? route = null) : base(route) { }
}

[AttributeUsage(AttributeTargets.Method)]
public class HttpPutAttribute : HttpMethodAttribute
{
    public HttpPutAttribute(string? route = null) : base(route) { }
}

[AttributeUsage(AttributeTargets.Method)]
public class HttpDeleteAttribute : HttpMethodAttribute
{
    public HttpDeleteAttribute(string? route = null) : base(route) { }
}

[AttributeUsage(AttributeTargets.Method)]
public class HttpPatchAttribute : HttpMethodAttribute
{
    public HttpPatchAttribute(string? route = null) : base(route) { }
}
