namespace ZeroReflection.Mapper
{
    /// <summary>
    /// Defines a mapping contract between a source type and a destination type.
    /// Implementations should provide logic to convert <typeparamref name="TSource"/> to <typeparamref name="TDestination"/>.
    /// </summary>
    public interface IMap<TSource, TDestination>
    {
        /// <summary>
        /// Maps an instance of <typeparamref name="TSource"/> to <typeparamref name="TDestination"/>.
        /// </summary>
        /// <param name="source">The source object to map from.</param>
        /// <returns>The mapped destination object.</returns>
        TDestination Map(TSource source);
    }
}