using System;

namespace ZeroReflection.Mapper
{
    /// <summary>
    /// Attribute to specify the destination property name for mapping.
    /// Use this on a source property to indicate which property in the destination type it should map to.
    /// </summary>
    /// <param name="destinationProperty">The name of the destination property to map to.</param>
    [AttributeUsage(AttributeTargets.Property)]
    public class MapToAttribute(string destinationProperty) : Attribute
    {
        /// <summary>
        /// Gets the name of the destination property this source property should map to.
        /// </summary>
        public string DestinationProperty { get; } = destinationProperty;
    }
}
