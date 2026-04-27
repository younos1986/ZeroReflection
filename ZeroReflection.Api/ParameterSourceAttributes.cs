using System;

namespace ZeroReflection.Api;

[AttributeUsage(AttributeTargets.Parameter)]
public class FromRouteAttribute : Attribute
{
    public string? Name { get; set; }
}

[AttributeUsage(AttributeTargets.Parameter)]
public class FromBodyAttribute : Attribute { }

[AttributeUsage(AttributeTargets.Parameter)]
public class FromQueryAttribute : Attribute
{
    public string? Name { get; set; }
}

[AttributeUsage(AttributeTargets.Parameter)]
public class FromHeaderAttribute : Attribute
{
    public string Name { get; }
    
    public FromHeaderAttribute(string name)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
    }
}
