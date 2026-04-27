using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using ZeroReflection.ApiGenerator.Models;

namespace ZeroReflection.ApiGenerator.Emit;

internal static class EndpointsJsonEmitter
{
    public static string Generate(IEnumerable<ControllerInfo> controllers)
    {
        var endpoints = new List<object>();

        foreach (var controller in controllers)
        {
            foreach (var endpoint in controller.Endpoints)
            {
                var fullRoute = CombineRoutes(controller.Route, endpoint.Route);
                
                endpoints.Add(new
                {
                    controller = controller.Name,
                    controllerNamespace = controller.Namespace,
                    controllerFullName = controller.FullName,
                    route = fullRoute,
                    method = endpoint.HttpMethod,
                    action = endpoint.MethodName,
                    responseType = endpoint.ResponseType,
                    summary = endpoint.Summary,
                    description = endpoint.Description,
                    parameters = endpoint.Parameters.Select(p => new
                    {
                        name = p.Name,
                        type = p.Type,
                        source = p.Source,
                        required = p.IsRequired
                    }).ToList()
                });
            }
        }

        var json = JsonSerializer.Serialize(new { endpoints }, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        return json;
    }

    private static string CombineRoutes(string baseRoute, string? endpointRoute)
    {
        if (string.IsNullOrEmpty(endpointRoute))
            return "/" + baseRoute.TrimStart('/');

        var combined = "/" + baseRoute.TrimStart('/').TrimEnd('/') + "/" + endpointRoute!.TrimStart('/');
        return combined;
    }
}
