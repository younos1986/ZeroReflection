using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace ZeroReflection.Mapper;

/// <summary>
/// Runtime helpers invoked by generated mappers for list/array projections.
/// Pulled into the runtime assembly so generator output can reference the type without extra dependencies.
/// </summary>
public static class MapCollectionHelpers
{
    // Caches for compiled delegates
    //private static readonly ConcurrentDictionary<(Type, Type), Delegate> SingleObjectMapCache = new();

    // Specialized cache for direct property/field mapping delegates
    private static readonly ConcurrentDictionary<(Type, Type), Delegate> DirectMapCache = new();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static List<TDest> MapList<TSrc, TDest>(List<TSrc> source, Func<TSrc, TDest> map)
    {
        if (source == null) return null;
        var count = source.Count;
        var dest = new List<TDest>(count);
        for (int i = 0; i < count; i++)
        {
            dest.Add(map(source[i]));
        }
        return dest;
    }
        
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static List<TDest> MapArrayToList<TSrc, TDest>(TSrc[] source, Func<TSrc, TDest> map)
    {
        if (source == null) return null;
        int len = source.Length;
        var resultList = new List<TDest>(len);
        for (int i = 0; i < len; i++)
        {
            resultList.Add(map(source[i]));
        }
        return resultList;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TDest[] MapArray<TSrc, TDest>(TSrc[] source, Func<TSrc, TDest> map)
    {
        if (source == null) return null;
        int len = source.Length;
        var result = new TDest[len];
        for (int i = 0; i < len; i++)
        {
            result[i] = map(source[i]);
        }
        return result;
    }
}