namespace ZeroReflection.Mapper;

public interface IGeneratedMappingDispatcher
{
    bool TryMapSingleObject(object source, System.Type sourceType, System.Type destType, out object result);
    bool TryMapList(object source, System.Type sourceType, System.Type destType, out object result);
    bool TryMapArray(object source, System.Type sourceType, System.Type destType, out object result);
}

internal sealed class NullGeneratedMappingDispatcher : IGeneratedMappingDispatcher
{
    public bool TryMapSingleObject(object source, System.Type sourceType, System.Type destType, out object result)
    { result = null!; return false; }

    public bool TryMapList(object source, System.Type sourceType, System.Type destType, out object result)
    { result = null!; return false; }
    
    public bool TryMapArray(object source, System.Type sourceType, System.Type destType, out object result)
    { result = null!; return false; }
}
