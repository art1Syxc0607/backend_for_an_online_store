using Application.Common.Caching;
using Application.DTOs.Product;
using Application.Interfaces.Caching;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Commands.Review;

public class DeleteReviewFilesCommand : IRequest<DeleteFilesResponseDto>, ICacheInvalidatingCommandWithCallback
{
    public int ReviewId { get; set; }
    public int UserId { get; set; }
    public List<string> FileUrls { get; init; } = new(); // список URL для удаления

    // ✅ Внутренний контекст — Handler заполняет, Behavior читает
    private readonly CacheInvalidationContext _cacheContext = new();

    // ✅ Пустой — Handler заполнит через SetCacheInvalidationData
    public IEnumerable<string> CachePrefixesToInvalidate
        => _cacheContext.GetPrefixes();

    // ✅ Handler вызывает этот метод
    public void SetCacheInvalidationData(CacheInvalidationContext context)
    {
        context.AddPrefixes(_cacheContext.GetPrefixes().ToArray());
    }

    // ✅ Helper для Handler'а
    internal void AddCachePrefix(string prefix)
        => _cacheContext.AddPrefix(prefix);
}
