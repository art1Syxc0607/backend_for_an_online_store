// Application/Behaviors/CachingBehavior.cs
using Application.Interfaces.Caching;
using Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Application.Behaviors;

/// <summary>
/// Pipeline behavior: автоматически кэширует запросы,
/// реализующие ICacheableQuery.
/// </summary>
public class CachingBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ICacheService _cache;
    private readonly ILogger<CachingBehavior<TRequest, TResponse>> _logger;

    public CachingBehavior(
        ICacheService cache,
        ILogger<CachingBehavior<TRequest, TResponse>> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        // ✅ Применяем только к ICacheableQuery
        if (request is not ICacheableQuery cacheableQuery)
        {
            return await next();
        }

        var requestName = typeof(TRequest).Name;
        var cacheKey = cacheableQuery.CacheKey;
        var duration = cacheableQuery.CacheDuration ?? TimeSpan.FromMinutes(10);

        // 1. Пробуем получить из кэша
        var cached = await _cache.GetAsync<TResponse>(cacheKey, ct);
        if (cached != null)
        {
            _logger.LogDebug(
                "🎯 Cache HIT: {RequestName} ({CacheKey})",
                requestName, cacheKey);
            return cached;
        }

        _logger.LogDebug(
            "❌ Cache MISS: {RequestName} ({CacheKey})",
            requestName, cacheKey);

        // 2. Вызываем handler
        var sw = Stopwatch.StartNew();
        var response = await next();
        sw.Stop();

        _logger.LogDebug(
            "⏱️ Handler {RequestName} executed in {ElapsedMs}ms",
            requestName, sw.ElapsedMilliseconds);

        // 3. Сохраняем в кэш (только если результат не null)
        if (response != null && duration > TimeSpan.Zero)
        {
            await _cache.SetAsync(cacheKey, response, duration, ct);

            _logger.LogDebug(
                "💾 Cache SET: {RequestName} ({CacheKey}), TTL {TTL}",
                requestName, cacheKey, duration);
        }

        return response;
    }
}