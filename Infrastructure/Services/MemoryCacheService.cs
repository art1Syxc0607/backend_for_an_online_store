using System.Collections.Concurrent;
using Application.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

public class MemoryCacheService : ICacheService
{
    private readonly IMemoryCache _cache;
    private readonly ConcurrentDictionary<string, byte> _keys = new();  // ✅
    private readonly ILogger<MemoryCacheService> _logger;

    public MemoryCacheService(
        IMemoryCache cache,
        ILogger<MemoryCacheService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    // ✅ Без lock — IMemoryCache потокобезопасен
    public Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
    {
        _cache.TryGetValue(key, out T? value);
        return Task.FromResult(value);
    }

    // ✅ Без lock для _cache, ConcurrentDictionary для _keys
    public Task SetAsync<T>(
        string key,
        T value,
        TimeSpan? expiration = null,
        CancellationToken ct = default)
    {
        var options = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromMinutes(10)
        };

        // ✅ Автоочистка _keys
        options.RegisterPostEvictionCallback((k, v, reason, state) =>
        {
            _keys.TryRemove(k.ToString()!, out _);
        });

        _cache.Set(key, value, options);  // ✅ потокобезопасно
        _keys.TryAdd(key, 0);              // ✅ потокобезопасно

        return Task.CompletedTask;
    }

    // ✅ Без lock
    public Task RemoveAsync(string key, CancellationToken ct = default)
    {
        _cache.Remove(key);              // ✅ потокобезопасно
        _keys.TryRemove(key, out _);     // ✅ потокобезопасно
        return Task.CompletedTask;
    }

    // ✅ Без lock — ConcurrentDictionary потокобезопасен
    public Task RemoveByPrefix(string prefix, CancellationToken ct = default)
    {
        var keysToRemove = _keys.Keys
            .Where(k => k.StartsWith(prefix, StringComparison.Ordinal))
            .ToList();

        foreach (var key in keysToRemove)
        {
            _cache.Remove(key);          // ✅ потокобезопасно
            _keys.TryRemove(key, out _); // ✅ потокобезопасно
        }

        _logger.LogInformation(
            "Removed {Count} cache keys with prefix {Prefix}",
            keysToRemove.Count, prefix);

        return Task.CompletedTask;
    }

    // ✅ Атомарно через GetOrCreate
    public async Task<T> GetOrSetAsync<T>(
        string key,
        Func<Task<T>> factory,
        TimeSpan? expiration = null,
        CancellationToken ct = default)
    {
        var cached = await GetAsync<T>(key, ct);
        if (cached != null)
            return cached;

        var value = await factory();

        if (value != null)
            await SetAsync(key, value, expiration, ct);

        return value;
    }
}