using Amazon.Lambda.APIGatewayEvents;
using AotSample.Commands;
using AotSample.Models.ViewModels;
using System.Text.Json.Serialization;
using ZeroReflection.Mediator;

namespace AotSample;

[JsonSerializable(typeof(APIGatewayProxyRequest))]
[JsonSerializable(typeof(APIGatewayProxyResponse))]
[JsonSerializable(typeof(Dictionary<string, string>))]
[JsonSerializable(typeof(UserModel))]
[JsonSerializable(typeof(CreateUserCommand))]
[JsonSerializable(typeof(UpdateUserCommand))]
[JsonSerializable(typeof(DeleteUserCommand))]
[JsonSerializable(typeof(GetUserQuery))]
[JsonSerializable(typeof(ListUsersQuery))]
[JsonSerializable(typeof(List<UserModel>))]
[JsonSerializable(typeof(Unit))]
[JsonSerializable(typeof(object))]
[JsonSerializable(typeof(Dictionary<string, object>))]
[JsonSerializable(typeof(string))]
[JsonSerializable(typeof(EndpointsResponse))]
[JsonSerializable(typeof(ErrorResponse))]
[JsonSerializable(typeof(List<EndpointInfo>))]
[JsonSerializable(typeof(EndpointInfo))]
[JsonSerializable(typeof(ParameterInfo))]
[JsonSourceGenerationOptions(WriteIndented = true)]
public partial class AotJsonContext : JsonSerializerContext
{
}

// Wrapper class for endpoints response (replaces anonymous type for AOT compatibility)
public class EndpointsResponse
{
    public List<EndpointInfo> Endpoints { get; set; } = new();
}

// Error response wrapper for AOT compatibility
public class ErrorResponse
{
    public string? Error { get; set; }
}

// Local copies of endpoint metadata types for AOT JSON serialization
public class EndpointInfo
{
    public string Controller { get; set; } = string.Empty;
    public string Route { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string ResponseType { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public List<ParameterInfo> Parameters { get; set; } = new();
}

public class ParameterInfo
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public bool IsRequired { get; set; }
}
