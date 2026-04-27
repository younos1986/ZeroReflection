using System.Collections.Generic;

namespace ZeroReflection.ApiGenerator.Models;

internal class ControllerInfo
{
    public string Name { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Namespace { get; set; } = string.Empty;
    public string Route { get; set; } = string.Empty;
    public List<EndpointInfo> Endpoints { get; set; } = new();
}

internal class EndpointInfo
{
    public string MethodName { get; set; } = string.Empty;
    public string HttpMethod { get; set; } = string.Empty;
    public string? Route { get; set; }
    public string? Summary { get; set; }
    public string? Description { get; set; }
    public string ResponseType { get; set; } = string.Empty;
    public List<ParameterInfo> Parameters { get; set; } = new();
}

internal class ParameterInfo
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Source { get; set; } = "body"; // body, route, query, header
    public bool IsRequired { get; set; }
}

// Models for TypeScript service generation
internal class EndpointDefinition
{
    public string ControllerName { get; set; } = string.Empty;
    public string Route { get; set; } = string.Empty;
    public string HttpMethod { get; set; } = string.Empty;
    public string ActionName { get; set; } = string.Empty;
    public string ResponseType { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public List<ParameterDefinition> Parameters { get; set; } = new();
}

internal class ParameterDefinition
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public bool IsRequired { get; set; }
}

