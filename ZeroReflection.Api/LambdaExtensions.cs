using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

#if NET8_0_OR_GREATER
using System.Text.Json;
using System.Text.Json.Serialization;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
#endif

namespace ZeroReflection.Api;

/// <summary>
/// Extension methods for integrating ZeroReflection API with AWS Lambda
/// </summary>
public static class LambdaExtensions
{
#if NET8_0_OR_GREATER
    /// <summary>
    /// Runs the application as an AWS Lambda function handler
    /// </summary>
    /// <typeparam name="TJsonContext">The System.Text.Json source-generated context</typeparam>
    /// <param name="serviceProvider">The dependency injection service provider</param>
    /// <param name="routeHandler">The route handler function that processes requests</param>
    /// <returns>A task representing the Lambda bootstrap</returns>
    public static async Task RunAsLambdaAsync<TJsonContext>(
        this IServiceProvider serviceProvider,
        Func<string, string, Dictionary<string, string>, Dictionary<string, string>, string?, IServiceProvider, CancellationToken, Task<RouteResult>> routeHandler)
        where TJsonContext : JsonSerializerContext, new()
    {
        var jsonContext = (JsonSerializerContext)Activator.CreateInstance(typeof(TJsonContext),
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase })!;

        var serializer = new Amazon.Lambda.Serialization.SystemTextJson.SourceGeneratorLambdaJsonSerializer<TJsonContext>(options =>
        {
            options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        });

        await Amazon.Lambda.RuntimeSupport.LambdaBootstrapBuilder.Create(
            async (APIGatewayProxyRequest request, ILambdaContext context) =>
                await HandleLambdaRequestAsync(request, context, serviceProvider, routeHandler, jsonContext),
            serializer
        ).Build().RunAsync();
    }

    private static async Task<APIGatewayProxyResponse> HandleLambdaRequestAsync(
        APIGatewayProxyRequest apiRequest,
        ILambdaContext context,
        IServiceProvider serviceProvider,
        Func<string, string, Dictionary<string, string>, Dictionary<string, string>, string?, IServiceProvider, CancellationToken, Task<RouteResult>> routeHandler,
        JsonSerializerContext jsonContext)
    {
        try
        {
            context.Logger.LogInformation($"Processing {apiRequest.HttpMethod} request to {apiRequest.Path}");

            var path = apiRequest.Path ?? "/";
            var method = apiRequest.HttpMethod ?? "GET";

            var routeValues = apiRequest.PathParameters != null
                ? new Dictionary<string, string>(apiRequest.PathParameters)
                : new Dictionary<string, string>();

            var queryValues = apiRequest.QueryStringParameters != null
                ? new Dictionary<string, string>(apiRequest.QueryStringParameters)
                : new Dictionary<string, string>();

            var body = apiRequest.Body;

            var filter = serviceProvider.GetService(typeof(ApiFilter)) as ApiFilter;
            var filterContext = new ApiFilterContext
            {
                Path = path,
                Method = method,
                QueryValues = queryValues,
                Body = body
            };

            if (filter != null)
                await filter.StartRequestAsync(filterContext);

            var result = await routeHandler(path, method, routeValues, queryValues, body, serviceProvider, CancellationToken.None);

            if (filter != null)
                await filter.StartResponseAsync(filterContext, result);

            var response = new APIGatewayProxyResponse
            {
                StatusCode = result.StatusCode,
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" },
                    { "Access-Control-Allow-Origin", "*" },
                    { "Access-Control-Allow-Headers", "Content-Type,Authorization" },
                    { "Access-Control-Allow-Methods", "GET,POST,PUT,DELETE,PATCH,OPTIONS" }
                }
            };

            if (result.SerializedJson != null)
            {
                response.Body = result.SerializedJson;
            }
            else if (result.Data != null)
            {
                var typeInfo = jsonContext.GetTypeInfo(result.Data.GetType());
                response.Body = typeInfo != null
                    ? JsonSerializer.Serialize(result.Data, typeInfo)
                    : "null";
            }
            else if (!string.IsNullOrEmpty(result.Error))
            {
                response.Body = $"{{\"error\":\"{result.Error}\"}}";
            }
            else
            {
                response.Body = "{}";
            }

            context.Logger.LogInformation($"Returning response with status code {response.StatusCode}");
            return response;
        }
        catch (Exception ex)
        {
            context.Logger.LogError($"Unhandled exception: {ex.Message}");
            context.Logger.LogError(ex.StackTrace);

            return new APIGatewayProxyResponse
            {
                StatusCode = 500,
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" }
                },
                Body = $"{{\"error\":\"Internal server error\",\"message\":\"{ex.Message.Replace("\"", "\\\"")}\"}}"
            };
        }
    }
#endif
}

/// <summary>
/// Represents the result of routing an API request
/// </summary>
public class RouteResult
{
    public int StatusCode { get; set; }
    public object? Data { get; set; }
    public string? Error { get; set; }

    public string? SerializedJson { get; set; }

    public static RouteResult Ok(object? data = null) => new() { StatusCode = 200, Data = data };
    public static RouteResult OkJson(string json) => new() { StatusCode = 200, SerializedJson = json };
    public static RouteResult NotFound() => new() { StatusCode = 404, Error = "Route not found" };
    public static RouteResult BadRequest(string error) => new() { StatusCode = 400, Error = error };
    public static RouteResult ServerError(string error) => new() { StatusCode = 500, Error = error };
}
