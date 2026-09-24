using Application.Common.Caching;
using Application.Interfaces.Caching;
using MediatR;

namespace Application.Commands.Review;

public class EditReviewCommamd : IRequest, ICacheInvalidatingCommand
{
    public int ReviewId { get; set; }
    public int UserId { get; init; }
    public string? NewText { get; init; }
    public int? NewRating { get; init; } // 1-5 stars

    // ✅ Контекст инвалидации
    private readonly CacheInvalidationContext _cacheContext = new();

    public IEnumerable<string> CachePrefixesToInvalidate
        => _cacheContext.GetPrefixes();

    // ✅ Handler вызывает этот метод
    internal void AddCachePrefix(string prefix)
        => _cacheContext.AddPrefix(prefix);
}
