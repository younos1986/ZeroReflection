using System;
using System.Collections;
using System.Runtime.CompilerServices;

namespace ZeroReflection.Mapper;

/// <summary>
/// Provides mapping capabilities between source and destination types.
/// </summary>
public class Mapper(IGeneratedMappingDispatcher dispatcher) : IMapper
{
    /// <summary>
    /// Maps the source object to a destination type. Use for mapping collections or arrays.
    /// </summary>
    /// <typeparam name="TDestination">The destination type.</typeparam>
    /// <param name="source">The source object to map.</param>
    /// <returns>The mapped object of type <typeparamref name="TDestination"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TDestination Map<TDestination>(object? source)
    {
        if (source is null)
            return default!;

        var sourceType = source.GetType();
        var destType = typeof(TDestination);

        return MapCollection<object, TDestination>(source, sourceType, destType);
    }


    /// <summary>
    /// Maps the source object of type <typeparamref name="TSource"/> to a destination type <typeparamref name="TDestination"/>.
    /// Use for mapping collections or arrays.
    /// </summary>
    /// <typeparam name="TSource">The source type.</typeparam>
    /// <typeparam name="TDestination">The destination type.</typeparam>
    /// <param name="source">The source object to map.</param>
    /// <returns>The mapped object of type <typeparamref name="TDestination"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TDestination Map<TSource, TDestination>(TSource source)
    {
        if (source is null)
            return default!;

        var sourceType = typeof(TSource);
        var destType = typeof(TDestination);

        return MapCollection<TSource, TDestination>(source, sourceType, destType);
    }

    /// <summary>
    /// Internal helper for mapping collections (arrays or lists) from source to destination type.
    /// Throws an exception if mapping is not possible.
    /// </summary>
    /// <typeparam name="TSource">The source type.</typeparam>
    /// <typeparam name="TDestination">The destination type.</typeparam>
    /// <param name="source">The source object to map.</param>
    /// <param name="sourceType">The type of the source object.</param>
    /// <param name="destType">The type of the destination object.</param>
    /// <returns>The mapped object of type <typeparamref name="TDestination"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private TDestination MapCollection<TSource, TDestination>(TSource source, Type sourceType, Type destType)
    {
        if (sourceType.IsArray)
        {
            if (dispatcher.TryMapArray(source!, sourceType, destType, out var arrResult))
                return (TDestination)arrResult;
        }
        else if (typeof(IList).IsAssignableFrom(sourceType))
        {
            if (dispatcher.TryMapList(source!, sourceType, destType, out var listResult))
                return (TDestination)listResult;
        }

        throw new Exception($"Cannot map from {sourceType.Name} to {destType.Name}.");
    }

    /// <summary>
    /// Maps a single object of type <typeparamref name="TSource"/> to a destination type <typeparamref name="TDestination"/>.
    /// Use for mapping individual objects, not collections.
    /// </summary>
    /// <typeparam name="TSource">The source type.</typeparam>
    /// <typeparam name="TDestination">The destination type.</typeparam>
    /// <param name="source">The source object to map.</param>
    /// <returns>The mapped object of type <typeparamref name="TDestination"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TDestination MapSingleObject<TSource, TDestination>(TSource source)
    {
        if (source is null)
            return default!;

        if (dispatcher.TryMapSingleObject(source!, typeof(TSource), typeof(TDestination), out var result))
            return (TDestination)result;

        throw new Exception($"Cannot map single object from {typeof(TSource).Name} to {typeof(TDestination).Name}.");
    }
}