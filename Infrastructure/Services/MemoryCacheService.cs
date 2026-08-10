using Application.Interfaces;
using Microsoft.Extensions.Caching.Memory;


namespace Infrastructure.Services;

public class MemoryCacheService : ICacheService
{
    private readonly IMemoryCache _cache;
    private static readonly HashSet<string> _keys = new HashSet<string>();
    private static readonly object _lock = new object();

    public MemoryCacheService(IMemoryCache cache)
    {
        _cache = cache;
    }

    public Task<T?> GetAsync<T>(string key)
    {
        _cache.TryGetValue(key, out T? value);
        return Task.FromResult(value);
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        var options = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromMinutes(10)
        };
        _cache.Set(key, value, options);

        lock (_lock)
        {
            _keys.Add(key);
        }

        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key)
    {
        _cache.Remove(key);
        lock (_lock)
        {
            _keys.Remove(key);
        }
        return Task.CompletedTask;
    }

    public Task RemoveByPrefix(string prefix)
    {
        lock (_lock)
        {
            var keysToRemove = _keys.Where(k => k.StartsWith(prefix)).ToList();
            foreach (var key in keysToRemove)
            {
                _cache.Remove(key);
                _keys.Remove(key);
            }
        }
        return Task.CompletedTask;
    }
}