using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace ZeroReflection.MapperGenerator.Extensions
{
    public static class TypeSymbolExtensions
    {
        public static List<IPropertySymbol> GetAllPublicProperties(this INamedTypeSymbol typeSymbol)
        {
            var result = new List<IPropertySymbol>();
            INamedTypeSymbol? currentType = typeSymbol;
            while (currentType != null && currentType.SpecialType != SpecialType.System_Object)
            {
                var props = currentType.GetMembers()
                    .OfType<IPropertySymbol>()
                    .Where(p => p.DeclaredAccessibility == Accessibility.Public);
                result.AddRange(props);
                currentType = currentType.BaseType;
            }
            return result;
        }

        public static bool HasIgnoreMapAttribute(this IPropertySymbol property)
        {
            return property.GetAttributes().Any(a => a.AttributeClass?.Name == "IgnoreMapAttribute");
        }

        public static AttributeData? GetMapToAttribute(this IPropertySymbol property)
        {
            return property.GetAttributes()
                .FirstOrDefault(a => a.AttributeClass?.Name == "MapToAttribute" && a.ConstructorArguments.Length == 1);
        }

        public static string GetMappedPropertyName(this IPropertySymbol property, string defaultName)
        {
            var mapToAttr = property.GetMapToAttribute();
            return mapToAttr?.ConstructorArguments[0].Value?.ToString() ?? defaultName;
        }

        public static IEnumerable<INamedTypeSymbol> GetAllTypes(this INamespaceSymbol namespaceSymbol)
        {
            foreach (var type in namespaceSymbol.GetTypeMembers())
            {
                yield return type;
            }

            foreach (var nestedNamespace in namespaceSymbol.GetNamespaceMembers())
            {
                foreach (var nestedType in nestedNamespace.GetAllTypes())
                {
                    yield return nestedType;
                }
            }
        }
    }
}
