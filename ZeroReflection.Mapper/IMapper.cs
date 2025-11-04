namespace ZeroReflection.Mapper;

public interface IMapper
{
    /// <summary>
    /// Maps the source object to a destination type. Use for mapping collections or arrays.
    /// </summary>
    /// <typeparam name="TDestination">The destination type.</typeparam>
    /// <param name="source">The source object to map.</param>
    /// <returns>The mapped object of type <typeparamref name="TDestination"/>.</returns>
    TDestination Map<TDestination>(object source);

    /// <summary>
    /// Maps the source object of type <typeparamref name="TSource"/> to a destination type <typeparamref name="TDestination"/>.
    /// Use for mapping collections or arrays.
    /// </summary>
    /// <typeparam name="TSource">The source type.</typeparam>
    /// <typeparam name="TDestination">The destination type.</typeparam>
    /// <param name="source">The source object to map.</param>
    /// <returns>The mapped object of type <typeparamref name="TDestination"/>.</returns>
    TDestination Map<TSource, TDestination>(TSource source);

    /// <summary>
    /// Maps a single object of type <typeparamref name="TSource"/> to a destination type <typeparamref name="TDestination"/>.
    /// Use for mapping individual objects, not collections.
    /// </summary>
    /// <typeparam name="TSource">The source type.</typeparam>
    /// <typeparam name="TDestination">The destination type.</typeparam>
    /// <param name="source">The source object to map.</param>
    /// <returns>The mapped object of type <typeparamref name="TDestination"/>.</returns>
    TDestination MapSingleObject<TSource, TDestination>(TSource source);
}