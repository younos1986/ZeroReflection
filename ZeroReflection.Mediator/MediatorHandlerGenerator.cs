using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace ZeroReflection.Mediator
{
    [Generator]
    public class MediatorHandlerGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var classDeclarations = context.SyntaxProvider
                .CreateSyntaxProvider(
                    predicate: (s, _) => s is ClassDeclarationSyntax cls && cls.BaseList != null,
                    transform: (ctx, _) => ctx.Node as ClassDeclarationSyntax)
                .Where(cls => cls != null);

            var compilationAndClasses = context.CompilationProvider.Combine(classDeclarations.Collect());

            // Aggregate the property value from all syntax trees
            var isEnabledProvider = context.AnalyzerConfigOptionsProvider
                .Select((opts, _) =>
                {
                    opts.GlobalOptions.TryGetValue("build_property.EnableZeroReflectionMediatorGeneratedCode", out var value);
                    return value ?? "true"; // Default to "false" if not set
                });

            var combined = compilationAndClasses.Combine(isEnabledProvider);

            context.RegisterSourceOutput(combined, (spc, source) =>
            {
                var compilationAndClassesValue = source.Left;
                var isEnabled = source.Right;
                var compilation = compilationAndClassesValue.Left;
                var classNodes = compilationAndClassesValue.Right;

                if (isEnabled.Equals(bool.FalseString, StringComparison.OrdinalIgnoreCase))
                    return; 

                var namespaces = CollectHandlerNamespaces(compilation, classNodes);
                var referencedRequestHandlers = ScanReferencedAssembliesForRequestHandlers(compilation, namespaces);
                var sb = new StringBuilder();
                GenerateUsings(sb, namespaces);
                GenerateRegistryClass(sb, classNodes, referencedRequestHandlers);
                spc.AddSource("MediatorHandlerRegistry.g.cs", SourceText.From(sb.ToString(), Encoding.UTF8));
            });
        }

        private static HashSet<string> CollectHandlerNamespaces(Compilation compilation, IEnumerable<ClassDeclarationSyntax> handlers)
        {
            var namespaces = new HashSet<string>();
            foreach (var handler in handlers)
            {
                var handlerNamespace = (handler.Parent as NamespaceDeclarationSyntax)?.Name.ToString();
                if (!string.IsNullOrWhiteSpace(handlerNamespace) && handlerNamespace != "<global namespace>")
                    namespaces.Add(handlerNamespace);

                if (handler.BaseList != null)
                {
                    foreach (var baseType in handler.BaseList.Types)
                    {
                        var typeSyntax = baseType.Type;
                        if (typeSyntax is GenericNameSyntax genericName)
                        {
                            foreach (var arg in genericName.TypeArgumentList.Arguments)
                            {
                                var model = compilation.GetSemanticModel(handler.SyntaxTree);
                                var symbol = model.GetSymbolInfo(arg).Symbol;
                                if (symbol != null && symbol.ContainingNamespace != null)
                                {
                                    var ns = symbol.ContainingNamespace.ToDisplayString();
                                    if (!string.IsNullOrEmpty(ns))
                                        namespaces.Add(ns);
                                }
                            }
                        }
                    }
                }
            }
            return namespaces;
        }

        private static void GenerateUsings(StringBuilder sb, HashSet<string> namespaces)
        {
            sb.AppendLine("using ZeroReflection.Mediator;");
            sb.AppendLine("using Microsoft.Extensions.DependencyInjection;");
            foreach (var ns in namespaces
                         .Where(q => !q.Contains("namespace"))
                         .OrderBy(x => x)
                    )
            {
                if (!string.IsNullOrWhiteSpace(ns) &&
                    ns != "ZeroReflection.Mediator" &&
                    ns != "ZeroReflection.Mediator.Contracts" &&
                    ns != "Microsoft.Extensions.DependencyInjection")
                {
                    sb.AppendLine($"using {ns};");
                }
            }
            sb.AppendLine();
        }

        private static void GenerateRegistryClass(StringBuilder sb, IEnumerable<ClassDeclarationSyntax> handlers, List<(string HandlerType, string RequestType, string ResponseType, string Namespace)> referencedRequestHandlers)
        {
            sb.AppendLine("namespace ZeroReflection.Mediator");
            sb.AppendLine("{");
            sb.AppendLine("    public static class MediatorHandlerRegistry");
            sb.AppendLine("    {");
            sb.AppendLine("        public static Microsoft.Extensions.DependencyInjection.IServiceCollection RegisterMediatorHandlers(this Microsoft.Extensions.DependencyInjection.IServiceCollection services)");
            sb.AppendLine("        {");
            sb.AppendLine("            // Auto-generated DI registration for handlers");
            sb.AppendLine("            // Handlers will be registered as transient");
            sb.AppendLine("            // Example: services.AddTransient<IRequestHandler<MyRequest, MyResponse>, MyRequestHandler>();");
            sb.AppendLine();
            sb.AppendLine($"            services.AddTransient<IMediator, MediatorImplementation>();");
            sb.AppendLine();
            sb.AppendLine("            // The following is generated for each handler:");
            sb.AppendLine();

            foreach (var handler in handlers)
            {
                var handlerName = handler.Identifier.Text;
                var interfaces = handler.BaseList.Types.Select(t => t.ToString()).ToList();
                foreach (var iface in interfaces)
                {
                    if (iface.StartsWith("IRequestHandler<"))
                    {
                        var args = iface.Substring("IRequestHandler<".Length).TrimEnd('>').Split(',');
                        if (args[1].Trim().Contains("<"))
                            sb.AppendLine($"            services.AddTransient<IRequestHandler<{args[0].Trim()}, {args[1].Trim()}>>, {handlerName}>();");
                        else
                            sb.AppendLine($"            services.AddTransient<IRequestHandler<{args[0].Trim()}, {args[1].Trim()}>, {handlerName}>();");
                    }
                    else if (iface.StartsWith("INotificationHandler<"))
                    {
                        var arg = iface.Substring("INotificationHandler<".Length).TrimEnd('>');
                        sb.AppendLine($"            services.AddTransient<INotificationHandler<{arg}>, {handlerName}>();");
                    }
                    else if (iface.StartsWith("IValidator<"))
                    {
                        var arg = iface.Substring("IValidator<".Length).TrimEnd('>');
                        sb.AppendLine($"            services.AddTransient<IValidator<{arg}>, {handlerName}>();");
                    }
                }
            }

            foreach (var handler in referencedRequestHandlers)
            {
                sb.AppendLine($"            services.AddTransient<IRequestHandler<{handler.RequestType}, {handler.ResponseType}>, {handler.HandlerType}>();");
            }

            sb.AppendLine();
            sb.AppendLine("            return services;");
            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine("}");
        }

        private static List<(string HandlerType, string RequestType, string ResponseType, string Namespace)> ScanReferencedAssembliesForRequestHandlers(Compilation compilation, HashSet<string> namespaces)
        {
            var referencedRequestHandlers = new List<(string HandlerType, string RequestType, string ResponseType, string Namespace)>();
            var iRequestSymbol = compilation.GetTypeByMetadataName("ZeroReflection.Mediator.Contracts.IRequest`1");
            var iRequestHandlerSymbol = compilation.GetTypeByMetadataName("ZeroReflection.Mediator.Contracts.IRequestHandler`2");
            if (iRequestSymbol != null && iRequestHandlerSymbol != null)
            {
                foreach (var reference in compilation.References)
                {
                    var assemblySymbol = compilation.GetAssemblyOrModuleSymbol(reference) as IAssemblySymbol;
                    if (assemblySymbol != null)
                    {
                        foreach (var type in GetAllTypes(assemblySymbol.GlobalNamespace))
                        {
                            foreach (var iface in type.AllInterfaces)
                            {
                                if (iface.OriginalDefinition.Equals(iRequestHandlerSymbol, SymbolEqualityComparer.Default))
                                {
                                    var handlerType = type.ToDisplayString();
                                    var requestType = iface.TypeArguments[0].ToDisplayString();
                                    var responseType = iface.TypeArguments[1].ToDisplayString();
                                    var ns = type.ContainingNamespace.ToDisplayString();
                                    referencedRequestHandlers.Add((handlerType, requestType, responseType, ns));
                                }
                            }
                        }
                    }
                }
            }
            foreach (var handler in referencedRequestHandlers)
            {
                if (!namespaces.Contains(handler.Namespace))
                    namespaces.Add(handler.Namespace);
            }
            return referencedRequestHandlers;
        }

        // Helper to recursively get all types in a namespace
        private static IEnumerable<INamedTypeSymbol> GetAllTypes(INamespaceSymbol @namespace)
        {
            foreach (var member in @namespace.GetMembers())
            {
                if (member is INamespaceSymbol ns)
                {
                    foreach (var type in GetAllTypes(ns))
                        yield return type;
                }
                else if (member is INamedTypeSymbol type)
                {
                    yield return type;
                }
            }
        }
    }
}
