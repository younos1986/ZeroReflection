using Amazon.Lambda.APIGatewayEvents;
using Api.Models;
using System.Text.Json.Serialization;

namespace Api;

[JsonSerializable(typeof(APIGatewayProxyRequest))]
[JsonSerializable(typeof(APIGatewayProxyResponse))]
[JsonSerializable(typeof(Dictionary<string, string>))]
[JsonSerializable(typeof(User))]
[JsonSerializable(typeof(List<User>))]
[JsonSerializable(typeof(Order))]
[JsonSerializable(typeof(List<Order>))]
[JsonSerializable(typeof(bool))]
[JsonSerializable(typeof(FileUploadRequest))]
[JsonSerializable(typeof(FileUploadResponse))]
[JsonSerializable(typeof(FileDownloadResponse))]
[JsonSerializable(typeof(object))]
[JsonSerializable(typeof(string))]
[JsonSerializable(typeof(EndpointsResponse))]
[JsonSerializable(typeof(ErrorResponse))]
[JsonSerializable(typeof(List<EndpointInfo>))]
[JsonSerializable(typeof(EndpointInfo))]
[JsonSerializable(typeof(ParameterInfo))]
[JsonSourceGenerationOptions(WriteIndented = true)]
public partial class ApiJsonContext : JsonSerializerContext
{
}

public class EndpointsResponse
{
    public List<EndpointInfo> Endpoints { get; set; } = new();
}

public class ErrorResponse
{
    public string? Error { get; set; }
}

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
