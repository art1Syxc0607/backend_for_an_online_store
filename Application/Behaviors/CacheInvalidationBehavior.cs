using Application.Interfaces;
using Application.Interfaces.Caching;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Behaviors;

/// <summary>
/// Pipeline behavior: инвалидирует кэш после успешного
/// выполнения команды, реализующей ICacheInvalidatingCommand.
/// </summary>
public class CacheInvalidationBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ICacheService _cache;
    private readonly ILogger<CacheInvalidationBehavior<TRequest, TResponse>> _logger;

    public CacheInvalidationBehavior(
        ICacheService cache,
        ILogger<CacheInvalidationBehavior<TRequest, TResponse>> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        // ✅ Сначала выполняем handler
        var response = await next();

        // ✅ Потом инвалидируем (только если handler успешен)
        if (request is ICacheInvalidatingCommand invalidating)
        {
            var requestName = typeof(TRequest).Name;

            foreach (var prefix in invalidating.CachePrefixesToInvalidate)
            {
                await _cache.RemoveByPrefix(prefix, ct);

                _logger.LogInformation(
                    "🗑️ Cache INVALIDATED: {RequestName} → prefix {Prefix}",
                    requestName, prefix);
            }
        }

        return response;
    }
}