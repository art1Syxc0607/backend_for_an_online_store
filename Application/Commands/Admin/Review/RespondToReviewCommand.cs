using Application.Common.Caching;
using Application.Interfaces.Caching;
using MediatR;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Commands.Admin.Review;


public class RespondToReviewCommand : IRequest, ICacheInvalidatingCommand
{
    [Required]
    public int ReviewId { get; init; }
    [Required]
    public int AdminId { get; init; }
    [Required]
    public string Response { get; init; } = string.Empty;
    [Required]
    public string BaseUrl { get; init; } = string.Empty;


    // ✅ Контекст инвалидации
    private readonly CacheInvalidationContext _cacheContext = new();

    public IEnumerable<string> CachePrefixesToInvalidate
        => _cacheContext.GetPrefixes();

    internal void AddCachePrefix(string prefix)
        => _cacheContext.AddPrefix(prefix);
}