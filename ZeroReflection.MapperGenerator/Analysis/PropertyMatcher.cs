using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using ZeroReflection.MapperGenerator.Extensions;
using ZeroReflection.MapperGenerator.Models;

namespace ZeroReflection.MapperGenerator.Analysis
{
    public class PropertyMatcher
    {
        public List<PropertyMapping> MatchProperties(INamedTypeSymbol sourceType, INamedTypeSymbol destinationType)
        {
            var sourceProps = sourceType.GetAllPublicProperties();
            var destinationProps = destinationType.GetAllPublicProperties();
            var propertyMappings = new List<PropertyMapping>();

            foreach (var destProp in destinationProps)
            {
                if (destProp.HasIgnoreMapAttribute())
                {
                    // Add as unmappable with reason
                    propertyMappings.Add(new PropertyMapping
                    {
                        Name = destProp.Name,
                        Type = destProp.Type.ToString(),
                        SourcePropertyName = string.Empty,
                        SourceType = string.Empty,
                        IsMappable = false,
                        UnmappableReason = "Property has [IgnoreMap] attribute",
                        IsCollection = false,
                        MappingType = MappingType.Direct
                    });
                    continue;
                }

                var sourceProperty = FindMatchingSourceProperty(destProp, sourceProps);
                if (sourceProperty != null && !sourceProperty.HasIgnoreMapAttribute())
                {
                    var mapping = CreatePropertyMapping(destProp, sourceProperty);
                    propertyMappings.Add(mapping);
                }
                else
                {
                    // Add as unmappable with reason
                    var reason = GetUnmappableReason(destProp, sourceProps);
                    propertyMappings.Add(new PropertyMapping
                    {
                        Name = destProp.Name,
                        Type = destProp.Type.ToString(),
                        SourcePropertyName = string.Empty,
                        SourceType = string.Empty,
                        IsMappable = false,
                        UnmappableReason = reason,
                        IsCollection = false,
                        MappingType = MappingType.Direct
                    });
                }
            }

            return propertyMappings;
        }

        private string GetUnmappableReason(IPropertySymbol destinationProperty, List<IPropertySymbol> sourceProperties)
        {
            // Check if there's a property with the same name but incompatible type
            var sameNameProperty = sourceProperties.FirstOrDefault(p => p.Name == destinationProperty.Name);
            if (sameNameProperty != null)
            {
                if (sameNameProperty.HasIgnoreMapAttribute())
                {
                    return "Source property has [IgnoreMap] attribute";
                }
                if (!AreTypesCompatible(sameNameProperty.Type, destinationProperty.Type))
                {
                    return $"Type mismatch: source is '{sameNameProperty.Type}', destination is '{destinationProperty.Type}'";
                }
            }

            // Check if there's a MapTo attribute pointing to a non-existent or incompatible source
            var mapToAttr = destinationProperty.GetMapToAttribute();
            if (mapToAttr is not null)
            {
                var sourceName = mapToAttr.ConstructorArguments[0].Value?.ToString();
                var sourceProperty = sourceProperties.FirstOrDefault(p => p.Name == sourceName);
                if (sourceProperty == null)
                {
                    return $"MapTo attribute points to non-existent source property '{sourceName}'";
                }
                if (!AreTypesCompatible(sourceProperty.Type, destinationProperty.Type))
                {
                    return $"MapTo attribute points to incompatible type: source '{sourceName}' is '{sourceProperty.Type}', destination is '{destinationProperty.Type}'";
                }
            }

            // Check if there's a source property with MapTo pointing to this destination but incompatible type
            var sourceWithMapTo = sourceProperties.FirstOrDefault(p => 
                p.GetMapToAttribute() is { } attr && attr.ConstructorArguments[0].Value?.ToString() == destinationProperty.Name);
            if (sourceWithMapTo != null && !AreTypesCompatible(sourceWithMapTo.Type, destinationProperty.Type))
            {
                return $"Source property '{sourceWithMapTo.Name}' maps to this destination but has incompatible type '{sourceWithMapTo.Type}'";
            }

            // Default reason
            return "No matching source property found";
        }

        private IPropertySymbol? FindMatchingSourceProperty(IPropertySymbol destinationProperty, List<IPropertySymbol> sourceProperties)
        {
            // First, check if destination property has MapTo attribute pointing to source
            var mapToAttr = destinationProperty.GetMapToAttribute();
            if (mapToAttr is not null)
            {
                var sourceName = mapToAttr.ConstructorArguments[0].Value?.ToString();
                return sourceProperties.FirstOrDefault(p => 
                    p.Name == sourceName && 
                    AreTypesCompatible(p.Type, destinationProperty.Type));
            }

            // Then check for direct name match or source property with MapTo attribute
            return sourceProperties.FirstOrDefault(p =>
                (p.Name == destinationProperty.Name && AreTypesCompatible(p.Type, destinationProperty.Type)) ||
                (p.GetMapToAttribute() is { } attr && attr.ConstructorArguments[0].Value?.ToString() == destinationProperty.Name &&
                 AreTypesCompatible(p.Type, destinationProperty.Type)));
        }

        private bool AreTypesCompatible(ITypeSymbol sourceType, ITypeSymbol destinationType)
        {
            // Direct type match
            if (sourceType.ToString() == destinationType.ToString())
                return true;

            // Handle nullable to non-nullable conversions
            if (IsNullableToNonNullableCompatible(sourceType, destinationType))
                return true;

            // Check if both are collections and their element types are compatible
            if (IsCollectionType(sourceType) && IsCollectionType(destinationType))
            {
                var sourceElementType = GetCollectionElementType(sourceType);
                var destElementType = GetCollectionElementType(destinationType);
                return sourceElementType != null && destElementType != null &&
                       (sourceElementType.ToString() == destElementType.ToString() || 
                        CanBeDeepMapped(sourceElementType, destElementType));
            }

            // Check if types can be deep mapped (custom classes)
            return CanBeDeepMapped(sourceType, destinationType);
        }

        private bool IsNullableToNonNullableCompatible(ITypeSymbol sourceType, ITypeSymbol destinationType)
        {
            var sourceTypeString = sourceType.ToString();
            var destTypeString = destinationType.ToString();

            // Handle nullable reference types (string? -> string)
            if (sourceTypeString.EndsWith("?") && !destTypeString.EndsWith("?"))
            {
                var sourceWithoutNullable = sourceTypeString.TrimEnd('?');
                return sourceWithoutNullable == destTypeString;
            }

            // Handle non-nullable to nullable reference types (string -> string?)
            if (!sourceTypeString.EndsWith("?") && destTypeString.EndsWith("?"))
            {
                var destWithoutNullable = destTypeString.TrimEnd('?');
                return sourceTypeString == destWithoutNullable;
            }

            // Handle nullable value types (int? -> int)
            if (sourceType.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T)
            {
                if (sourceType is INamedTypeSymbol namedSourceType && namedSourceType.TypeArguments.Length == 1)
                {
                    var underlyingType = namedSourceType.TypeArguments[0];
                    return underlyingType.ToString() == destTypeString;
                }
            }

            // Handle non-nullable to nullable value types (int -> int?)
            if (destinationType.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T)
            {
                if (destinationType is INamedTypeSymbol namedDestType && namedDestType.TypeArguments.Length == 1)
                {
                    var underlyingType = namedDestType.TypeArguments[0];
                    return sourceTypeString == underlyingType.ToString();
                }
            }

            return false;
        }

        private bool IsCollectionType(ITypeSymbol type)
        {
            if (type is IArrayTypeSymbol)
                return true;

            if (type is INamedTypeSymbol namedType)
            {
                // Check for ICollection<T>, IList<T>, List<T>, etc.
                return namedType.AllInterfaces.Any(i => 
                    i.Name == "ICollection" || i.Name == "IList" || i.Name == "IEnumerable") &&
                    namedType.TypeArguments.Length == 1;
            }

            return false;
        }

        private ITypeSymbol? GetCollectionElementType(ITypeSymbol type)
        {
            if (type is IArrayTypeSymbol arrayType)
                return arrayType.ElementType;

            if (type is INamedTypeSymbol namedType && namedType.TypeArguments.Length == 1)
                return namedType.TypeArguments[0];

            return null;
        }

        private bool CanBeDeepMapped(ITypeSymbol sourceType, ITypeSymbol destinationType)
        {
            // Skip primitive types and system types
            if (IsPrimitiveOrSystemType(sourceType) || IsPrimitiveOrSystemType(destinationType))
                return false;

            // Both should be custom classes/structs that can potentially be mapped
            return sourceType is INamedTypeSymbol && destinationType is INamedTypeSymbol;
        }

        private bool IsPrimitiveOrSystemType(ITypeSymbol type)
        {
            var typeName = type.ToString();
            return type.SpecialType != SpecialType.None ||
                   typeName.StartsWith("System.") ||
                   typeName == "string" ||
                   typeName == "object";
        }

        private PropertyMapping CreatePropertyMapping(IPropertySymbol destinationProperty, IPropertySymbol sourceProperty)
        {
            var sourcePropertyName = GetEffectiveSourcePropertyName(destinationProperty, sourceProperty);
            var mappingType = DetermineMappingType(sourceProperty.Type, destinationProperty.Type);
            
            return new PropertyMapping
            {
                Name = destinationProperty.Name,
                Type = destinationProperty.Type.ToString(),
                SourcePropertyName = sourcePropertyName ?? destinationProperty.Name,
                MappingType = mappingType,
                SourceType = sourceProperty.Type.ToString(),
                IsCollection = IsCollectionType(destinationProperty.Type),
                CollectionElementType = GetCollectionElementType(destinationProperty.Type)?.ToString() ?? string.Empty,
                SourceCollectionElementType = GetCollectionElementType(sourceProperty.Type)?.ToString() ?? string.Empty
            };
        }

        private MappingType DetermineMappingType(ITypeSymbol sourceType, ITypeSymbol destinationType)
        {
            // Direct assignment (same types)
            if (sourceType.ToString() == destinationType.ToString())
                return MappingType.Direct;

            // Check for nullable conversions
            var sourceTypeString = sourceType.ToString();
            var destTypeString = destinationType.ToString();
            
            // Nullable to non-nullable conversion
            if ((sourceTypeString.EndsWith("?") && !destTypeString.EndsWith("?")) ||
                (sourceType.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T))
            {
                return MappingType.NullableToNonNullable;
            }
            
            // Non-nullable to nullable conversion
            if ((!sourceTypeString.EndsWith("?") && destTypeString.EndsWith("?")) ||
                (destinationType.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T))
            {
                return MappingType.NonNullableToNullable;
            }

            // Collection mapping
            if (IsCollectionType(sourceType) && IsCollectionType(destinationType))
            {
                var sourceElementType = GetCollectionElementType(sourceType);
                var destElementType = GetCollectionElementType(destinationType);
                
                if (sourceElementType?.ToString() == destElementType?.ToString())
                    return MappingType.CollectionDirect;
                
                if (sourceElementType != null && destElementType != null && CanBeDeepMapped(sourceElementType, destElementType))
                    return MappingType.CollectionDeep;
            }

            // Deep object mapping
            if (CanBeDeepMapped(sourceType, destinationType))
                return MappingType.Deep;

            return MappingType.Direct; // Fallback
        }

        private string GetEffectiveSourcePropertyName(IPropertySymbol destinationProperty, IPropertySymbol sourceProperty)
        {
            // If destination has MapTo attribute, use the specified source name
            var destMapToAttr = destinationProperty.GetMapToAttribute();
            if (destMapToAttr is not null)
            {
                var sourceName = destMapToAttr.ConstructorArguments[0].Value?.ToString();
                if (sourceName != null)
                    return sourceName;
            }

            // If source has MapTo attribute pointing to destination, use source name
            var srcMapToAttr = sourceProperty.GetMapToAttribute();
            if (srcMapToAttr is { } attr && attr.ConstructorArguments[0].Value?.ToString() == destinationProperty.Name)
            {
                return sourceProperty.Name;
            }

            // Default to destination property name for direct matches
            return destinationProperty.Name;
        }
    }
}
