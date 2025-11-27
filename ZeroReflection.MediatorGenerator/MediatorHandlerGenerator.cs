﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using ZeroReflection.MediatorGenerator.Emit;

namespace ZeroReflection.MediatorGenerator
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
                    return value ?? "true"; // Default to "true" if not set
                });

            // Check if switch dispatcher should be used
            var useSwitchDispatcherProvider = context.AnalyzerConfigOptionsProvider
                .Select((opts, _) =>
                {
                    opts.GlobalOptions.TryGetValue("build_property.ZeroReflectionMediatorUseSwitchDispatcher", out var value);
                    return value ?? "true"; // Default to "true" if not set
                });

            var combinedWithSettings = compilationAndClasses.Combine(isEnabledProvider).Combine(useSwitchDispatcherProvider);

            context.RegisterSourceOutput(combinedWithSettings, (spc, source) =>
            {
                var compilationAndClassesValue = source.Left.Left;
                var isEnabled = source.Left.Right;
                var useSwitchDispatcherStr = source.Right;
                var compilation = compilationAndClassesValue.Left;
                var classNodes = compilationAndClassesValue.Right;

                if (isEnabled.Equals(bool.FalseString, StringComparison.OrdinalIgnoreCase))
                    return;

                bool useSwitchDispatcher = !useSwitchDispatcherStr.Equals(bool.FalseString, StringComparison.OrdinalIgnoreCase);

                var namespaces = CollectHandlerNamespaces(compilation, classNodes);
                var handlers = CollectHandlers(compilation, classNodes, namespaces);
                var validators = CollectValidators(compilation, classNodes, namespaces);
                
                var sb = new StringBuilder();
                GenerateUsings(sb, namespaces);
                GenerateRegistryClass(sb, handlers, validators);
                spc.AddSource("MediatorHandlerRegistry.g.cs", SourceText.From(sb.ToString(), Encoding.UTF8));

                // Generate dispatcher
                if (handlers.Count > 0)
                {
                    var dispatcherCode = DispatcherEmitter.Build(handlers, namespaces, useSwitchDispatcher);
                    spc.AddSource("GeneratedMediatorDispatcher.g.cs", SourceText.From(dispatcherCode, Encoding.UTF8));
                }
            });
        }

        private static HashSet<string> CollectHandlerNamespaces(Compilation compilation, IEnumerable<ClassDeclarationSyntax?> handlers)
        {
            var namespaces = new HashSet<string>();
            foreach (var handler in handlers)
            {
                if (handler == null)
                    continue;
                    
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

        private static List<HandlerInfo> CollectHandlers(Compilation compilation, IEnumerable<ClassDeclarationSyntax?> classNodes, HashSet<string> namespaces)
        {
            var handlers = new List<HandlerInfo>();
            
            // Collect from current project
            foreach (var handler in classNodes)
            {
                if (handler == null || handler.BaseList == null)
                    continue;
                    
                var handlerName = handler.Identifier.Text;
                var interfaces = handler.BaseList.Types.Select(t => t.ToString()).ToList();
                
                foreach (var iface in interfaces)
                {
                    if (iface.StartsWith("IRequestHandler<"))
                    {
                        var args = iface.Substring("IRequestHandler<".Length).TrimEnd('>').Split(',');
                        if (args.Length == 2)
                        {
                            handlers.Add(new HandlerInfo
                            {
                                HandlerType = handlerName,
                                RequestType = args[0].Trim(),
                                ResponseType = args[1].Trim(),
                                Namespace = (handler.Parent as NamespaceDeclarationSyntax)?.Name.ToString() ?? ""
                            });
                        }
                    }
                }
            }

            // Scan referenced assemblies
            var referencedHandlers = ScanReferencedAssembliesForRequestHandlers(compilation, namespaces);
            handlers.AddRange(referencedHandlers);

            return handlers;
        }

        private static List<ValidatorInfo> CollectValidators(Compilation compilation, IEnumerable<ClassDeclarationSyntax?> classNodes, HashSet<string> namespaces)
        {
            var validators = new List<ValidatorInfo>();
            
            // Collect from current project
            foreach (var validator in classNodes)
            {
                if (validator == null || validator.BaseList == null)
                    continue;
                    
                var validatorName = validator.Identifier.Text;
                var interfaces = validator.BaseList.Types.Select(t => t.ToString()).ToList();
                
                foreach (var iface in interfaces)
                {
                    if (iface.StartsWith("IValidator<"))
                    {
                        var requestType = iface.Substring("IValidator<".Length).TrimEnd('>').Trim();
                        validators.Add(new ValidatorInfo
                        {
                            ValidatorType = validatorName,
                            RequestType = requestType,
                            Namespace = (validator.Parent as NamespaceDeclarationSyntax)?.Name.ToString() ?? ""
                        });
                    }
                }
            }

            // Scan referenced assemblies
            var referencedValidators = ScanReferencedAssembliesForValidators(compilation, namespaces);
            validators.AddRange(referencedValidators);

            return validators;
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

        private static void GenerateRegistryClass(StringBuilder sb, List<HandlerInfo> handlers, List<ValidatorInfo> validators)
        {
            sb.AppendLine("namespace ZeroReflection.Mediator");
            sb.AppendLine("{");
            sb.AppendLine("    public static class MediatorHandlerRegistry");
            sb.AppendLine("    {");
            sb.AppendLine("        public static Microsoft.Extensions.DependencyInjection.IServiceCollection RegisterZeroReflectionMediatorHandlers(this Microsoft.Extensions.DependencyInjection.IServiceCollection services)");
            sb.AppendLine("        {");
            sb.AppendLine("            // Auto-generated DI registration for handlers");
            sb.AppendLine($"            services.AddTransient<IMediator, MediatorImplementation>();");
            
            if (handlers.Count > 0)
            {
                sb.AppendLine($"            services.AddSingleton<IGeneratedMediatorDispatcher, ZeroReflection.Mediator.Generated.GeneratedMediatorDispatcher>();");
            }
            else
            {
                sb.AppendLine($"            services.AddSingleton<IGeneratedMediatorDispatcher, NullGeneratedMediatorDispatcher>();");
            }
            
            sb.AppendLine();

            var processedHandlers = new HashSet<string>();
            
            foreach (var handler in handlers)
            {
                var key = $"{handler.HandlerType}|{handler.RequestType}|{handler.ResponseType}";
                if (processedHandlers.Contains(key))
                    continue;
                    
                processedHandlers.Add(key);
                
                sb.AppendLine($"            services.AddTransient<IRequestHandler<{handler.RequestType}, {handler.ResponseType}>, {handler.HandlerType}>();");
            }

            sb.AppendLine();
            
            // Register validators
            var processedValidators = new HashSet<string>();
            foreach (var validator in validators)
            {
                var key = $"{validator.ValidatorType}|{validator.RequestType}";
                if (processedValidators.Contains(key))
                    continue;
                    
                processedValidators.Add(key);
                
                sb.AppendLine($"            services.AddTransient<IValidator<{validator.RequestType}>, {validator.ValidatorType}>();");
            }

            sb.AppendLine();
            sb.AppendLine("            return services;");
            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine("}");
        }

        private static List<HandlerInfo> ScanReferencedAssembliesForRequestHandlers(Compilation compilation, HashSet<string> namespaces)
        {
            var referencedRequestHandlers = new List<HandlerInfo>();
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
                                    referencedRequestHandlers.Add(new HandlerInfo
                                    {
                                        HandlerType = handlerType,
                                        RequestType = requestType,
                                        ResponseType = responseType,
                                        Namespace = ns
                                    });
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

        private static List<ValidatorInfo> ScanReferencedAssembliesForValidators(Compilation compilation, HashSet<string> namespaces)
        {
            var referencedValidators = new List<ValidatorInfo>();
            var iValidatorSymbol = compilation.GetTypeByMetadataName("ZeroReflection.Mediator.Contracts.IValidator`1");
            if (iValidatorSymbol != null)
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
                                if (iface.OriginalDefinition.Equals(iValidatorSymbol, SymbolEqualityComparer.Default))
                                {
                                    var validatorType = type.ToDisplayString();
                                    var requestType = iface.TypeArguments[0].ToDisplayString();
                                    var ns = type.ContainingNamespace.ToDisplayString();
                                    referencedValidators.Add(new ValidatorInfo
                                    {
                                        ValidatorType = validatorType,
                                        RequestType = requestType,
                                        Namespace = ns
                                    });
                                }
                            }
                        }
                    }
                }
            }
            foreach (var validator in referencedValidators)
            {
                if (!namespaces.Contains(validator.Namespace))
                    namespaces.Add(validator.Namespace);
            }
            return referencedValidators;
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

    internal class ValidatorInfo
    {
        public string ValidatorType { get; set; } = string.Empty;
        public string RequestType { get; set; } = string.Empty;
        public string Namespace { get; set; } = string.Empty;
    }
}
