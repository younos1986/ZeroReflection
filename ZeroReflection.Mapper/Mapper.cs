using System;
using System.Collections;
using System.Runtime.CompilerServices;

#nullable enable

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
        return (TDestination)MapInternal(source, source.GetType(), typeof(TDestination));
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
        return (TDestination)MapInternal(source, typeof(TSource), typeof(TDestination));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private object MapInternal(object source, Type sourceType, Type destType)
    {
        if (sourceType.IsArray)
        {
            if (dispatcher.TryMapArray(source, sourceType, destType, out var arrResult))
                return arrResult;
        }
        else if (typeof(IList).IsAssignableFrom(sourceType))
        {
            if (dispatcher.TryMapList(source, sourceType, destType, out var listResult))
                return listResult;
        }
        else
        {
            if (dispatcher.TryMapSingleObject(source, sourceType, destType, out var singleResult))
                return singleResult;
        }

        throw new InvalidOperationException($"Cannot map from {sourceType.FullName} to {destType.FullName}.");
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
        if (dispatcher.TryMapSingleObject(source, typeof(TSource), typeof(TDestination), out var result))
            return (TDestination)result;
        throw new InvalidOperationException($"Cannot map single object from {typeof(TSource).FullName} to {typeof(TDestination).FullName}.");
    }
}