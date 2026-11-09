using Application.Common.Caching;
using Application.Interfaces.Caching;
using MediatR;

namespace Application.Commands.Review;

public class DeleteReviewCommand : IRequest, ICacheInvalidatingCommand
{
    public int ReviewId { get; set; }
    public int UserId { get; set; }

    // ✅ Внутренний контекст — Handler заполняет, Behavior читает
    private readonly CacheInvalidationContext _cacheContext = new();

    public IEnumerable<string> CachePrefixesToInvalidate
        => _cacheContext.GetPrefixes();

    // ✅ Helper для Handler'а
    internal void AddCachePrefix(string prefix)
        => _cacheContext.AddPrefix(prefix);
}
