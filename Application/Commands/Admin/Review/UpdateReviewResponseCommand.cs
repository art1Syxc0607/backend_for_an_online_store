using Application.Common.Caching;
using Application.Interfaces.Caching;
using MediatR;

namespace Application.Commands.Admin.Review;


public class UpdateReviewResponseCommand : IRequest, ICacheInvalidatingCommand
{
    public int ReviewId { get; init; }
    public int AdminId { get; init; }  // ✅ добавили для проверки прав
    public string NewResponse { get; init; } = string.Empty;

    // ✅ Контекст инвалидации
    private readonly CacheInvalidationContext _cacheContext = new();

    public IEnumerable<string> CachePrefixesToInvalidate
        => _cacheContext.GetPrefixes();

    internal void AddCachePrefix(string prefix)
        => _cacheContext.AddPrefix(prefix);
}