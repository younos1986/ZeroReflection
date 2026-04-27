using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZeroReflection.ApiGenerator.Models;
using Microsoft.CodeAnalysis;

namespace ZeroReflection.ApiGenerator.Emit;

internal static class TypeScriptServiceEmitter
{
    public static string GenerateTypeScriptService(List<EndpointDefinition> endpoints, Compilation? compilation = null)
    {
        var sb = new StringBuilder();
        
        // File header
        sb.AppendLine("/* Auto-generated TypeScript service from ZeroReflection API */");
        sb.AppendLine($"/* Generated at: {System.DateTime.UtcNow:yyyy-MM-ddTHH:mm:ssZ} */");
        sb.AppendLine();
        sb.AppendLine("import { Injectable } from '@angular/core';");
        sb.AppendLine("import { HttpClient, HttpParams } from '@angular/common/http';");
        sb.AppendLine("import { Observable } from 'rxjs';");
        sb.AppendLine();
        
        // Extract unique types for DTOs
        var typeMap = ExtractTypes(endpoints);
        
        // Generate DTOs with proper properties
        sb.AppendLine("// ==================== DTOs ====================");
        sb.AppendLine();
        
        if (compilation != null)
        {
            var generatedTypes = new HashSet<string>();
            foreach (var typeName in typeMap.Keys.OrderBy(t => t))
            {
                GenerateTypeScriptInterface(sb, typeName, compilation, generatedTypes);
            }
        }
        else
        {
            // Fallback to placeholder interfaces
            foreach (var typeName in typeMap.Keys.OrderBy(t => t))
            {
                var cleanName = GetCleanTypeName(typeName);
                sb.AppendLine($"export interface {cleanName} {{");
                sb.AppendLine("  // TODO: Add properties based on your backend model");
                sb.AppendLine("  [key: string]: any;");
                sb.AppendLine("}");
                sb.AppendLine();
            }
        }
        
        // Generate service
        sb.AppendLine("// ==================== Service ====================");
        sb.AppendLine();
        sb.AppendLine("@Injectable({");
        sb.AppendLine("  providedIn: 'root'");
        sb.AppendLine("})");
        sb.AppendLine("export class ApiService {");
        sb.AppendLine("  private baseUrl = '/api'; // Configure your API base URL");
        sb.AppendLine();
        sb.AppendLine("  constructor(private http: HttpClient) {}");
        sb.AppendLine();
        
        // Group endpoints by controller
        var groupedEndpoints = endpoints
            .GroupBy(e => e.ControllerName)
            .OrderBy(g => g.Key)
            .ToList();
        
        // Generate methods for each controller group
        for (int i = 0; i < groupedEndpoints.Count; i++)
        {
            var group = groupedEndpoints[i];
            if (i > 0) sb.AppendLine();
            sb.AppendLine($"  // ==================== {group.Key} ====================");
            sb.AppendLine();
            
            foreach (var endpoint in group)
            {
                GenerateServiceMethod(sb, endpoint);
                sb.AppendLine();
            }
        }
        
        sb.AppendLine("}");
        
        return sb.ToString();
    }
    
    private static Dictionary<string, bool> ExtractTypes(List<EndpointDefinition> endpoints)
    {
        var types = new Dictionary<string, bool>();
        
        foreach (var endpoint in endpoints)
        {
            // Extract from response type (output)
            if (!string.IsNullOrEmpty(endpoint.ResponseType))
            {
                var responseType = ExtractTypeName(endpoint.ResponseType);
                if (!string.IsNullOrEmpty(responseType) && !IsSystemType(responseType))
                {
                    types[responseType] = true;
                }
            }
            
            // Extract from ALL parameters (inputs) - body, route, query
            foreach (var param in endpoint.Parameters)
            {
                // Skip CancellationToken as it's not part of the API contract
                if (param.Source == "cancellationToken")
                    continue;
                    
                var paramType = ExtractTypeName(param.Type);
                
                // For body parameters, always include complex types
                if (param.Source == "body" && !string.IsNullOrEmpty(paramType) && !IsSystemType(paramType))
                {
                    types[paramType] = true;
                }
                
                // For route and query parameters, include if they are complex types
                // (though typically these are primitives like string, int, etc.)
                if ((param.Source == "route" || param.Source == "query") && 
                    !string.IsNullOrEmpty(paramType) && !IsSystemType(paramType))
                {
                    types[paramType] = true;
                }
            }
        }
        
        return types;
    }
    
    private static string ExtractTypeName(string fullType)
    {
        // Extract type from Task<T>, List<T>, etc.
        var genericStart = fullType.IndexOf('<');
        var genericEnd = fullType.LastIndexOf('>');
        
        if (genericStart >= 0 && genericEnd > genericStart)
        {
            return fullType.Substring(genericStart + 1, genericEnd - genericStart - 1);
        }
        
        return fullType;
    }
    
    private static bool IsSystemType(string typeName)
    {
        var systemTypes = new[] { "string", "int", "bool", "double", "float", "decimal", "DateTime", "Guid", "Unit" };
        return systemTypes.Any(t => typeName.Contains(t)) || 
               typeName.StartsWith("System.") ||
               typeName.Contains("ZeroReflection.Mediator.Unit");
    }
    
    private static string GetCleanTypeName(string fullTypeName)
    {
        // Get last part of namespace
        var parts = fullTypeName.Split('.');
        return parts[parts.Length - 1];
    }
    
    private static void GenerateServiceMethod(StringBuilder sb, EndpointDefinition endpoint)
    {
        var methodName = GetMethodName(endpoint.ActionName);
        var routeParams = endpoint.Parameters.Where(p => p.Source == "route").ToList();
        var queryParams = endpoint.Parameters.Where(p => p.Source == "query").ToList();
        var bodyParams = endpoint.Parameters.Where(p => p.Source == "body").ToList();
        
        // Build parameter list
        var parameters = new List<string>();
        foreach (var p in routeParams)
        {
            parameters.Add($"{p.Name}: {MapTypeToTS(p.Type)}");
        }
        foreach (var p in queryParams)
        {
            parameters.Add($"{p.Name}?: {MapTypeToTS(p.Type)}");
        }
        foreach (var p in bodyParams)
        {
            parameters.Add($"{p.Name}: {GetCleanTypeName(p.Type)}");
        }
        
        // Determine return type
        var returnType = GetReturnType(endpoint.ResponseType);
        
        // Add JSDoc comment
        if (!string.IsNullOrEmpty(endpoint.Summary))
        {
            sb.AppendLine("  /**");
            sb.AppendLine($"   * {endpoint.Summary}");
            sb.AppendLine("   */");
        }
        
        // Method signature
        sb.AppendLine($"  {methodName}({string.Join(", ", parameters)}): Observable<{returnType}> {{");
        
        // Build URL
        var url = endpoint.Route;
        foreach (var p in routeParams)
        {
            url = url.Replace($"{{{p.Name}}}", $"${{{p.Name}}}");
        }
        sb.AppendLine($"    const url = `${{this.baseUrl}}{url}`;");
        
        // Add query parameters if any
        if (queryParams.Any())
        {
            sb.AppendLine("    let params = new HttpParams();");
            foreach (var p in queryParams)
            {
                sb.AppendLine($"    if ({p.Name} !== undefined) {{");
                sb.AppendLine($"      params = params.set('{p.Name}', {p.Name}.toString());");
                sb.AppendLine("    }");
            }
        }
        
        // HTTP call
        var httpMethod = endpoint.HttpMethod.ToLowerInvariant();
        if (bodyParams.Any())
        {
            var bodyParam = bodyParams[0].Name;
            if (queryParams.Any())
            {
                sb.AppendLine($"    return this.http.{httpMethod}<{returnType}>(url, {bodyParam}, {{ params }});");
            }
            else
            {
                sb.AppendLine($"    return this.http.{httpMethod}<{returnType}>(url, {bodyParam});");
            }
        }
        else
        {
            if (queryParams.Any())
            {
                sb.AppendLine($"    return this.http.{httpMethod}<{returnType}>(url, {{ params }});");
            }
            else
            {
                sb.AppendLine($"    return this.http.{httpMethod}<{returnType}>(url);");
            }
        }
        
        sb.AppendLine("  }");
    }
    
    private static string GetMethodName(string actionName)
    {
        // Convert action name to camelCase
        if (string.IsNullOrEmpty(actionName)) return "unknownMethod";
        return char.ToLowerInvariant(actionName[0]) + actionName.Substring(1);
    }
    
    private static string MapTypeToTS(string csharpType)
    {
        if (csharpType.Contains("string")) return "string";
        if (csharpType.Contains("int") || csharpType.Contains("Int32") || csharpType.Contains("Int64")) return "number";
        if (csharpType.Contains("double") || csharpType.Contains("float") || csharpType.Contains("decimal")) return "number";
        if (csharpType.Contains("bool")) return "boolean";
        if (csharpType.Contains("DateTime")) return "Date | string";
        if (csharpType.Contains("Guid")) return "string";
        return "any";
    }
    
    private static string GetReturnType(string responseType)
    {
        if (string.IsNullOrEmpty(responseType)) return "void";
        
        // Handle Task<T>
        var innerType = ExtractTypeName(responseType);
        
        // Handle List<T>
        if (innerType.Contains("List<"))
        {
            var listStart = innerType.IndexOf("List<") + 5;
            var listEnd = innerType.LastIndexOf('>');
            if (listEnd > listStart)
            {
                var listType = innerType.Substring(listStart, listEnd - listStart);
                return $"{GetCleanTypeName(listType)}[]";
            }
        }
        
        // Handle Unit (void)
        if (innerType.Contains("Unit"))
        {
            return "void";
        }
        
        // Regular type
        if (!IsSystemType(innerType))
        {
            return GetCleanTypeName(innerType);
        }
        
        return MapTypeToTS(innerType);
    }
    
    private static void GenerateTypeScriptInterface(StringBuilder sb, string fullTypeName, Compilation compilation, HashSet<string> generatedTypes)
    {
        var cleanName = GetCleanTypeName(fullTypeName);
        
        // Avoid generating the same type multiple times
        if (generatedTypes.Contains(cleanName))
            return;
            
        generatedTypes.Add(cleanName);
        
        // Try to find the type symbol in the compilation
        var typeSymbol = FindTypeSymbol(fullTypeName, compilation);
        
        if (typeSymbol == null || typeSymbol.TypeKind != TypeKind.Class)
        {
            // Fallback to placeholder
            sb.AppendLine($"export interface {cleanName} {{");
            sb.AppendLine("  // Type not found in compilation, using placeholder");
            sb.AppendLine("  [key: string]: any;");
            sb.AppendLine("}");
            sb.AppendLine();
            return;
        }
        
        // Add comment about the model
        sb.AppendLine($"/** {cleanName} model from {typeSymbol.ContainingNamespace} */");
        sb.AppendLine($"export interface {cleanName} {{");
        
        // Get all public properties
        var properties = typeSymbol.GetMembers()
            .OfType<IPropertySymbol>()
            .Where(p => p.DeclaredAccessibility == Accessibility.Public && !p.IsStatic)
            .OrderBy(p => p.Name)
            .ToList();
        
        if (properties.Count == 0)
        {
            sb.AppendLine("  // No public properties found");
            sb.AppendLine("  [key: string]: any;");
        }
        else
        {
            foreach (var prop in properties)
            {
                var tsType = MapCSharpTypeToTypeScript(prop.Type);
                var isOptional = prop.NullableAnnotation == NullableAnnotation.Annotated || !prop.IsRequired;
                var optionalMarker = isOptional ? "?" : "";
                
                sb.AppendLine($"  {ToCamelCase(prop.Name)}{optionalMarker}: {tsType};");
            }
        }
        
        sb.AppendLine("}");
        sb.AppendLine();
        
        // Generate nested complex types
        foreach (var prop in properties)
        {
            if (!IsSystemType(prop.Type.ToDisplayString()) && prop.Type.TypeKind == TypeKind.Class)
            {
                var nestedTypeName = prop.Type.ToDisplayString();
                if (!generatedTypes.Contains(GetCleanTypeName(nestedTypeName)))
                {
                    GenerateTypeScriptInterface(sb, nestedTypeName, compilation, generatedTypes);
                }
            }
            
            // Handle collection element types
            if (prop.Type is INamedTypeSymbol namedType && namedType.TypeArguments.Length > 0)
            {
                foreach (var typeArg in namedType.TypeArguments)
                {
                    if (!IsSystemType(typeArg.ToDisplayString()) && typeArg.TypeKind == TypeKind.Class)
                    {
                        var elementTypeName = typeArg.ToDisplayString();
                        if (!generatedTypes.Contains(GetCleanTypeName(elementTypeName)))
                        {
                            GenerateTypeScriptInterface(sb, elementTypeName, compilation, generatedTypes);
                        }
                    }
                }
            }
        }
    }
    
    private static INamedTypeSymbol? FindTypeSymbol(string fullTypeName, Compilation compilation)
    {
        // Try to get the type symbol from the compilation
        var typeSymbol = compilation.GetTypeByMetadataName(fullTypeName);
        
        if (typeSymbol != null)
            return typeSymbol;
        
        // If not found, try all assemblies
        foreach (var assembly in compilation.References)
        {
            var assemblySymbol = compilation.GetAssemblyOrModuleSymbol(assembly) as IAssemblySymbol;
            if (assemblySymbol == null)
                continue;
                
            typeSymbol = assemblySymbol.GetTypeByMetadataName(fullTypeName);
            if (typeSymbol != null)
                return typeSymbol;
        }
        
        return null;
    }
    
    private static string MapCSharpTypeToTypeScript(ITypeSymbol typeSymbol)
    {
        var typeName = typeSymbol.ToDisplayString();
        
        // Handle nullable types
        if (typeSymbol.NullableAnnotation == NullableAnnotation.Annotated && typeSymbol is INamedTypeSymbol namedType)
        {
            var underlyingType = namedType.TypeArguments.FirstOrDefault();
            if (underlyingType != null)
                typeName = underlyingType.ToDisplayString();
        }
        
        // Map primitive types
        if (typeName.Contains("string") || typeName.Contains("String"))
            return "string";
        if (typeName.Contains("int") || typeName.Contains("Int32") || typeName.Contains("Int64") || 
            typeName.Contains("long") || typeName.Contains("short") || typeName.Contains("byte"))
            return "number";
        if (typeName.Contains("double") || typeName.Contains("float") || typeName.Contains("decimal") || typeName.Contains("Decimal"))
            return "number";
        if (typeName.Contains("bool") || typeName.Contains("Boolean"))
            return "boolean";
        if (typeName.Contains("DateTime") || typeName.Contains("DateTimeOffset") || typeName.Contains("DateOnly"))
            return "string"; // ISO date string
        if (typeName.Contains("Guid"))
            return "string";
        if (typeName.Contains("TimeSpan") || typeName.Contains("TimeOnly"))
            return "string";
            
        // Handle collections
        if (typeSymbol is IArrayTypeSymbol arrayType)
        {
            var elementType = MapCSharpTypeToTypeScript(arrayType.ElementType);
            return $"{elementType}[]";
        }
        
        if (typeSymbol is INamedTypeSymbol namedTypeSymbol)
        {
            // Handle List<T>, IList<T>, IEnumerable<T>, etc.
            if (namedTypeSymbol.TypeArguments.Length > 0)
            {
                var originalDef = namedTypeSymbol.OriginalDefinition.ToDisplayString();
                if (originalDef.Contains("List<") || originalDef.Contains("IEnumerable<") || 
                    originalDef.Contains("ICollection<") || originalDef.Contains("IList<"))
                {
                    var elementType = MapCSharpTypeToTypeScript(namedTypeSymbol.TypeArguments[0]);
                    return $"{elementType}[]";
                }
                
                // Handle Dictionary<TKey, TValue>
                if (originalDef.Contains("Dictionary<") || originalDef.Contains("IDictionary<"))
                {
                    return "{ [key: string]: any }";
                }
            }
        }
        
        // For complex types, return the clean type name
        return GetCleanTypeName(typeName);
    }
    
    private static string ToCamelCase(string name)
    {
        if (string.IsNullOrEmpty(name))
            return name;
        return char.ToLowerInvariant(name[0]) + name.Substring(1);
    }
}
