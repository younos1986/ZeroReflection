using System;

namespace ZeroReflection.Mapper
{
    /// <summary>
    /// Marks a method as a custom mapper between two types
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class CustomMappingAttribute(Type sourceType, Type destinationType) : Attribute
    {
        public Type SourceType { get; } = sourceType;
        public Type DestinationType { get; } = destinationType;
    }
    
    /// <summary>
    /// Marks a method as a custom property mapper
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class CustomPropertyMappingAttribute(Type sourceType, Type destinationType, string propertyName) : Attribute
    {
        public Type SourceType { get; } = sourceType;
        public Type DestinationType { get; } = destinationType;
        public string PropertyName { get; } = propertyName;
    }
}
