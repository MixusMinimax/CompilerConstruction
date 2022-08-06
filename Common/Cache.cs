namespace Common;

public interface ICache<TKey, TValue> where TKey : notnull
{
    public bool TryGetValue(TKey key, out TValue value);
    public TValue? Get(TKey key);
    public TValue Set(TKey key, TValue value);
}

public class Cache<TKey, TValue> : ICache<TKey, TValue> where TKey : notnull
{
    private readonly int _cacheMaxSize;

    private readonly Dictionary<TKey, (
        DateTime LastUpdate,
        TValue Value
        )> _cache = new();

    public Cache(int cacheMaxSize = 256)
    {
        _cacheMaxSize = cacheMaxSize;
    }

    public bool TryGetValue(TKey key, out TValue value)
    {
        value = default!;
        if (!_cache.TryGetValue(key, out var result)) return false;
        _cache[key] = (DateTime.Now, result.Value);
        value = result.Value;
        return true;
    }

    public TValue? Get(TKey key)
    {
        return TryGetValue(key, out var result) ? result : default;
    }

    public TValue Set(TKey key, TValue value)
    {
        if (_cache.Count >= _cacheMaxSize && _cache.Count > 0)
            _cache.Remove(_cache.MinBy(e => e.Value.LastUpdate).Key);
        return (_cache[key] = (DateTime.Now, value)).Value;
    }
}