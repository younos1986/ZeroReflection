using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using ZeroReflection.ApiGenerator.Emit;
using ZeroReflection.ApiGenerator.Models;

namespace ZeroReflection.ApiGenerator;

[Generator]
public class EndpointDiscoveryGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var controllerClasses = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: (node, _) => node is ClassDeclarationSyntax cls && cls.AttributeLists.Count > 0,
                transform: (ctx, _) => GetControllerInfo(ctx))
            .Where(c => c != null);

        var compilationAndControllers = context.CompilationProvider
            .Combine(controllerClasses.Collect());

        var configProvider = context.AnalyzerConfigOptionsProvider
            .Select((opts, _) =>
            {
                opts.GlobalOptions.TryGetValue("build_property.EnableZeroReflectionApiGeneratedCode", out var isEnabled);
                opts.GlobalOptions.TryGetValue("build_property.RootNamespace", out var rootNamespace);
                opts.GlobalOptions.TryGetValue("build_property.ZeroReflectionJsonContext", out var jsonContext);
                return (IsEnabled: isEnabled ?? "true", RootNamespace: rootNamespace ?? "AotSample", JsonContext: jsonContext ?? "AotJsonContext");
            });

        var combined = compilationAndControllers.Combine(configProvider);

        context.RegisterSourceOutput(combined, (spc, source) =>
        {
            var ((compilation, controllers), config) = source;

            if (config.IsEnabled.Equals(bool.FalseString, StringComparison.OrdinalIgnoreCase))
                return;

            var rootNamespace = config.RootNamespace;
            var jsonContextName = config.JsonContext;

            var validControllers = controllers.Where(c => c != null).Cast<ControllerInfo>().ToList();
            
            if (!validControllers.Any())
                return;

            var namespaces = CollectNamespaces(validControllers);

            // Convert to EndpointDefinition for code generation
            var endpointDefinitions = ConvertToEndpointDefinitions(validControllers);

            // Generate endpoints metadata as C# code
            var endpointsMetadata = GenerateEndpointsMetadata(validControllers, rootNamespace);
            spc.AddSource("GeneratedEndpointsMetadata.g.cs", SourceText.From(endpointsMetadata, Encoding.UTF8));

            var dispatcher = RouteDispatcherEmitter.Generate(validControllers, namespaces, rootNamespace, jsonContextName);
            spc.AddSource("GeneratedApiRouter.g.cs", SourceText.From(dispatcher, Encoding.UTF8));

            // Generate route matching helper
            var routeHelper = GenerateRouteHelper(rootNamespace);
            spc.AddSource("RouteHelper.g.cs", SourceText.From(routeHelper, Encoding.UTF8));

            // Generate Swagger UI middleware
            var swaggerUI = SwaggerUIEmitter.GenerateSwaggerUI(rootNamespace, jsonContextName);
            spc.AddSource("SwaggerUIMiddleware.g.cs", SourceText.From(swaggerUI, Encoding.UTF8));

            // Generate HTTP server helper
            var httpServer = HttpServerEmitter.GenerateHttpServer(rootNamespace, jsonContextName);
            spc.AddSource("SimpleHttpServer.g.cs", SourceText.From(httpServer, Encoding.UTF8));

            // Note: TypeScript service generation should be done as a separate build step
            // Source generators should only generate C# code
            // Note: JSON serialization context is manually defined in AotJsonContext.cs
            // to avoid conflicts with System.Text.Json source generator
        });
    }

    private static ControllerInfo? GetControllerInfo(GeneratorSyntaxContext context)
    {
        var classDeclaration = (ClassDeclarationSyntax)context.Node;
        var model = context.SemanticModel;
        var classSymbol = model.GetDeclaredSymbol(classDeclaration) as INamedTypeSymbol;

        if (classSymbol == null)
            return null;

        var apiControllerAttr = classSymbol.GetAttributes()
            .FirstOrDefault(a => a.AttributeClass?.Name == "ApiControllerAttribute");

        if (apiControllerAttr == null)
            return null;

        // Support both ZeroReflection [ApiController("route")] and ASP.NET Core [Route("route")]
        var route = apiControllerAttr.ConstructorArguments.FirstOrDefault().Value?.ToString() ?? "";
        if (string.IsNullOrEmpty(route))
        {
            var routeAttr = classSymbol.GetAttributes()
                .FirstOrDefault(a => a.AttributeClass?.Name == "RouteAttribute");
            route = routeAttr?.ConstructorArguments.FirstOrDefault().Value?.ToString() ?? "";
        }

        var controllerInfo = new ControllerInfo
        {
            Name = classSymbol.Name,
            FullName = classSymbol.ToDisplayString(),
            Namespace = classSymbol.ContainingNamespace.ToDisplayString(),
            Route = route
        };

        foreach (var member in classSymbol.GetMembers().OfType<IMethodSymbol>())
        {
            if (member.MethodKind != MethodKind.Ordinary)
                continue;

            var httpMethodAttr = member.GetAttributes()
                .FirstOrDefault(a => a.AttributeClass?.Name?.StartsWith("Http") == true &&
                                    a.AttributeClass?.Name?.EndsWith("Attribute") == true);

            if (httpMethodAttr == null)
                continue;

            var httpMethod = httpMethodAttr.AttributeClass!.Name.Replace("Attribute", "").Replace("Http", "").ToUpperInvariant();
            var endpointRoute = httpMethodAttr.ConstructorArguments.FirstOrDefault().Value?.ToString();
            var summary = httpMethodAttr.NamedArguments.FirstOrDefault(n => n.Key == "Summary").Value.Value?.ToString();

            var endpoint = new EndpointInfo
            {
                MethodName = member.Name,
                HttpMethod = httpMethod,
                Route = endpointRoute,
                Summary = summary,
                ResponseType = member.ReturnType.ToDisplayString()
            };

            foreach (var param in member.Parameters)
            {
                var paramSource = "body";

                var fromRouteAttr = param.GetAttributes().FirstOrDefault(a => a.AttributeClass?.Name == "FromRouteAttribute");
                var fromBodyAttr = param.GetAttributes().FirstOrDefault(a => a.AttributeClass?.Name == "FromBodyAttribute");
                var fromQueryAttr = param.GetAttributes().FirstOrDefault(a => a.AttributeClass?.Name == "FromQueryAttribute");
                var fromServicesAttr = param.GetAttributes().FirstOrDefault(a => a.AttributeClass?.Name == "FromServicesAttribute");

                if (fromRouteAttr != null)
                    paramSource = "route";
                else if (fromBodyAttr != null)
                    paramSource = "body";
                else if (fromQueryAttr != null)
                    paramSource = "query";
                else if (fromServicesAttr != null)
                    paramSource = "services";
                else if (param.Type.Name == "CancellationToken")
                    paramSource = "cancellationToken";
                else if (endpointRoute != null &&
                         (endpointRoute.Contains("{" + param.Name + "}") ||
                          endpointRoute.Contains("{" + param.Name + ":")))
                    paramSource = "route";

                endpoint.Parameters.Add(new ParameterInfo
                {
                    Name = param.Name,
                    Type = param.Type.ToDisplayString(),
                    Source = paramSource,
                    IsRequired = param.NullableAnnotation != NullableAnnotation.Annotated
                });
            }

            controllerInfo.Endpoints.Add(endpoint);
        }

        return controllerInfo;
    }

    private static HashSet<string> CollectNamespaces(List<ControllerInfo> controllers)
    {
        var namespaces = new HashSet<string>();
        
        foreach (var controller in controllers)
        {
            if (!string.IsNullOrEmpty(controller.Namespace))
                namespaces.Add(controller.Namespace);

            foreach (var endpoint in controller.Endpoints)
            {
                foreach (var param in endpoint.Parameters)
                {
                    var ns = ExtractNamespace(param.Type);
                    if (!string.IsNullOrEmpty(ns))
                        namespaces.Add(ns);
                }
            }
        }

        return namespaces;
    }

    private static List<EndpointDefinition> ConvertToEndpointDefinitions(List<ControllerInfo> controllers)
    {
        var definitions = new List<EndpointDefinition>();
        
        foreach (var controller in controllers)
        {
            foreach (var endpoint in controller.Endpoints)
            {
                var fullRoute = CombineRoutes(controller.Route, endpoint.Route);
                var definition = new EndpointDefinition
                {
                    ControllerName = controller.Name,
                    Route = fullRoute,
                    HttpMethod = endpoint.HttpMethod,
                    ActionName = endpoint.MethodName,
                    ResponseType = endpoint.ResponseType,
                    Summary = endpoint.Summary ?? string.Empty
                };
                
                foreach (var param in endpoint.Parameters)
                {
                    definition.Parameters.Add(new ParameterDefinition
                    {
                        Name = param.Name,
                        Type = param.Type,
                        Source = param.Source,
                        IsRequired = param.IsRequired
                    });
                }
                
                definitions.Add(definition);
            }
        }
        
        return definitions;
    }

    private static string ExtractNamespace(string fullTypeName)
    {
        var lastDot = fullTypeName.LastIndexOf('.');
        if (lastDot > 0)
            return fullTypeName.Substring(0, lastDot);
        return string.Empty;
    }

    private static string GenerateRouteHelper(string rootNamespace)
    {
        var sb = new StringBuilder();
        sb.AppendLine("// <auto-generated />");
        sb.AppendLine("using System.Collections.Generic;");
        sb.AppendLine("using System.Linq;");
        sb.AppendLine();
        sb.AppendLine($"namespace {rootNamespace}.Generated;");
        sb.AppendLine();
        sb.AppendLine("internal static class RouteHelper");
        sb.AppendLine("{");
        sb.AppendLine("    public static bool MatchesRoute(string actualPath, string pattern, out Dictionary<string, string> routeValues)");
        sb.AppendLine("    {");
        sb.AppendLine("        routeValues = new Dictionary<string, string>();");
        sb.AppendLine("        ");
        sb.AppendLine("        var actualParts = actualPath.Trim('/').Split('/');");
        sb.AppendLine("        var patternParts = pattern.Trim('/').Split('/');");
        sb.AppendLine("        ");
        sb.AppendLine("        if (actualParts.Length != patternParts.Length)");
        sb.AppendLine("            return false;");
        sb.AppendLine("        ");
        sb.AppendLine("        for (int i = 0; i < patternParts.Length; i++)");
        sb.AppendLine("        {");
        sb.AppendLine("            var patternPart = patternParts[i];");
        sb.AppendLine("            var actualPart = actualParts[i];");
        sb.AppendLine("            ");
        sb.AppendLine("            if (patternPart.StartsWith(\"{\") && patternPart.EndsWith(\"}\"))");
        sb.AppendLine("            {");
        sb.AppendLine("                var paramName = patternPart.Trim('{', '}');");
        sb.AppendLine("                routeValues[paramName] = actualPart;");
        sb.AppendLine("            }");
        sb.AppendLine("            else if (!patternPart.Equals(actualPart, System.StringComparison.OrdinalIgnoreCase))");
        sb.AppendLine("            {");
        sb.AppendLine("                return false;");
        sb.AppendLine("            }");
        sb.AppendLine("        }");
        sb.AppendLine("        ");
        sb.AppendLine("        return true;");
        sb.AppendLine("    }");
        sb.AppendLine("}");

        return sb.ToString();
    }

    private static string GenerateEndpointsMetadata(List<ControllerInfo> controllers, string rootNamespace)
    {
        var sb = new StringBuilder();
        sb.AppendLine("// <auto-generated />");
        sb.AppendLine("#nullable enable");
        sb.AppendLine("using System.Collections.Generic;");
        sb.AppendLine();
        sb.AppendLine($"namespace {rootNamespace}.Generated;");
        sb.AppendLine();
        sb.AppendLine("public static class EndpointsMetadata");
        sb.AppendLine("{");
        sb.AppendLine("    public static List<EndpointInfo> GetAllEndpoints()");
        sb.AppendLine("    {");
        sb.AppendLine("        return new List<EndpointInfo>");
        sb.AppendLine("        {");

        foreach (var controller in controllers)
        {
            foreach (var endpoint in controller.Endpoints)
            {
                var fullRoute = CombineRoutes(controller.Route, endpoint.Route);
                sb.AppendLine("            new EndpointInfo");
                sb.AppendLine("            {");
                sb.AppendLine($"                Controller = \"{controller.Name}\",");
                sb.AppendLine($"                Route = \"{fullRoute}\",");
                sb.AppendLine($"                Method = \"{endpoint.HttpMethod}\",");
                sb.AppendLine($"                Action = \"{endpoint.MethodName}\",");
                sb.AppendLine($"                ResponseType = \"{endpoint.ResponseType}\",");
                sb.AppendLine($"                Summary = {(endpoint.Summary != null ? $"\"{endpoint.Summary}\"" : "null")},");
                sb.AppendLine("                Parameters = new List<ParameterInfo>");
                sb.AppendLine("                {");
                
                foreach (var param in endpoint.Parameters)
                {
                    sb.AppendLine($"                    new ParameterInfo {{ Name = \"{param.Name}\", Type = \"{param.Type}\", Source = \"{param.Source}\", IsRequired = {param.IsRequired.ToString().ToLower()} }},");
                }
                
                sb.AppendLine("                }");
                sb.AppendLine("            },");
            }
        }

        sb.AppendLine("        };");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    public class EndpointInfo");
        sb.AppendLine("    {");
        sb.AppendLine("        public string Controller { get; set; } = string.Empty;");
        sb.AppendLine("        public string Route { get; set; } = string.Empty;");
        sb.AppendLine("        public string Method { get; set; } = string.Empty;");
        sb.AppendLine("        public string Action { get; set; } = string.Empty;");
        sb.AppendLine("        public string ResponseType { get; set; } = string.Empty;");
        sb.AppendLine("        public string? Summary { get; set; }");
        sb.AppendLine("        public List<ParameterInfo> Parameters { get; set; } = new();");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    public class ParameterInfo");
        sb.AppendLine("    {");
        sb.AppendLine("        public string Name { get; set; } = string.Empty;");
        sb.AppendLine("        public string Type { get; set; } = string.Empty;");
        sb.AppendLine("        public string Source { get; set; } = string.Empty;");
        sb.AppendLine("        public bool IsRequired { get; set; }");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    public class EndpointsResponse");
        sb.AppendLine("    {");
        sb.AppendLine("        public List<EndpointInfo> Endpoints { get; set; } = new();");
        sb.AppendLine("    }");
        sb.AppendLine("}");

        return sb.ToString();
    }

    private static string CombineRoutes(string baseRoute, string? endpointRoute)
    {
        if (string.IsNullOrEmpty(endpointRoute))
            return "/" + baseRoute.TrimStart('/');

        var combined = "/" + baseRoute.TrimStart('/').TrimEnd('/') + "/" + endpointRoute!.TrimStart('/');
        return combined;
    }
}
