using System.Collections.Generic;

namespace ZeroReflection.MapperGenerator.Models
{
    public class MappingInfo
    {
        public string Source { get; set; } = string.Empty;
        public string Destination { get; set; } = string.Empty;
        public string SourceNamespace { get; set; } = string.Empty;
        public string DestinationNamespace { get; set; } = string.Empty;
        public List<PropertyMapping> Properties { get; set; } = new List<PropertyMapping>();
        
        // Custom mapping support
        public bool HasCustomMapping { get; set; }
        public string? CustomMappingMethod { get; set; }
        public bool IsMappable { get; set; } = true;
        public string? UnmappableReason { get; set; }
        
        // Custom property mapping support
        public bool IsCustomMapped { get; set; }
        public string? CustomMappingExpression { get; set; }

        // Custom mapping profile support
        public string? CustomMappingProfileFullName { get; set; }
        public bool CustomMappingIsStatic { get; set; }

        // Projection function support
        // ReSharper disable UnusedAutoPropertyAccessor.Global
#pragma warning disable CS0649, IDE0051, IDE0052
        public bool EnableProjectionFunctions { get; set; }
#pragma warning restore CS0649, IDE0051, IDE0052
        // ReSharper restore UnusedAutoPropertyAccessor.Global
        
        // Switch dispatcher support
        public bool UseSwitchDispatcher { get; set; }
        
        // Throw if property mapping missing support
        public bool ThrowIfPropertyMissing { get; set; }
        
        // Pass Analyzers Log to generated code to debug 
        public string? LoggedString { get; set; }
    }

    public class PropertyMapping
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string SourcePropertyName { get; set; } = string.Empty;
        public MappingType MappingType { get; set; }
        public string SourceType { get; set; } = string.Empty;
        // ReSharper disable UnusedAutoPropertyAccessor.Global
#pragma warning disable CS0649, IDE0051, IDE0052
        public bool IsCollection { get; set; }
#pragma warning restore CS0649, IDE0051, IDE0052
        // ReSharper restore UnusedAutoPropertyAccessor.Global
        public string? CollectionElementType { get; set; }
        public string? SourceCollectionElementType { get; set; }
        // Added per-property mapping metadata used by analyzer and generator
        public bool IsMappable { get; set; } = true;
        public string? UnmappableReason { get; set; }
        public bool IsCustomMapped { get; set; }
        public string? CustomMappingExpression { get; set; }
    }

    public enum MappingType
    {
        Direct,           // source.Property = dest.Property
        Deep,            // custom mapping method needed
        CollectionDirect, // collection with same element types
        CollectionDeep,   // collection needing element mapping
        NullableToNonNullable, // nullable source to non-nullable destination
        NonNullableToNullable  // non-nullable source to nullable destination
    }
}
