using System.Collections.Generic;

namespace ZeroReflection.Mapper.CodeGeneration.Models
{
    public class MappingInfo
    {
        public string Source { get; set; }
        public string Destination { get; set; }
        public string SourceNamespace { get; set; }
        public string DestinationNamespace { get; set; }
        public List<PropertyMapping> Properties { get; set; } = new List<PropertyMapping>();
        
        // Custom mapping support
        public bool HasCustomMapping { get; set; }
        public string CustomMappingMethod { get; set; }
        public bool IsMappable { get; set; } = true;
        public string UnmappableReason { get; set; }
        
        // Custom property mapping support
        public bool IsCustomMapped { get; set; }
        public string CustomMappingExpression { get; set; }

        // Custom mapping profile support
        public string CustomMappingProfileFullName { get; set; }
        public bool CustomMappingIsStatic { get; set; }

        // Projection function support
        public bool EnableProjectionFunctions { get; set; }
        
        // Switch dispatcher support
        public bool UseSwitchDispatcher { get; set; }
        
        // Pass Analyzers Log to generated code to debug 
        public string LoggedString { get; set; }
    }

    public class PropertyMapping
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string SourcePropertyName { get; set; }
        public MappingType MappingType { get; set; }
        public string SourceType { get; set; }
        public bool IsCollection { get; set; }
        public string CollectionElementType { get; set; }
        public string SourceCollectionElementType { get; set; }
        // Added per-property mapping metadata used by analyzer and generator
        public bool IsMappable { get; set; } = true;
        public string UnmappableReason { get; set; }
        public bool IsCustomMapped { get; set; }
        public string CustomMappingExpression { get; set; }
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
