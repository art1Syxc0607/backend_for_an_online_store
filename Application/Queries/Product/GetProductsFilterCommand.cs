using Application.Common;
using Application.DTOs.Product;
using Application.Enums;
using Application.Interfaces.Caching;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Queries.Product;

public class GetProductsFilterCommand : IRequest<PagedResult<ProductResponseDto>>, ICacheableQuery
{
    public string? SearchText { get; init; }
    public int? CategoryId { get; init; }
    public decimal? PriceLimitMax { get; init; }
    public decimal? PriceLimitMin { get; init; }
    public bool? OnlyAvailable { get; init; } = true;
    public int? PageNumber { get; init; } = 1;
    public int? PageSize { get; init; } = 20;
    public SortProductBy? SortBy { get; init; }
    public bool SortDesc { get; init; } = true;
    //public bool? OnlyOutOfUserCart { get; init; } = false;

    // ✅ Кэш-ключ
    public string CacheKey
    {
        get
        {
            // ✅ Нормализуем так же, как это делает handler/repository,
            // чтобы null и 1 (а также размер > 50) не создавали дубли ключей.
            var pageNumber = Math.Max(1, PageNumber ?? 1);
            var pageSize = Math.Clamp(PageSize ?? 20, 1, 50);

            var parts = new List<string>
            {
                CacheKeys.ProductsPrefix + "list",
                $"p{pageNumber}",
                $"s{pageSize}"
            };

            if (CategoryId.HasValue) parts.Add($"cat{CategoryId}");
            if (PriceLimitMin.HasValue) parts.Add($"min{PriceLimitMin}");
            if (PriceLimitMax.HasValue) parts.Add($"max{PriceLimitMax}");
            if (!string.IsNullOrWhiteSpace(SearchText))
                parts.Add($"q{SearchText.ToLowerInvariant()}");

            // ✅ OnlyAvailable влияет на выборку — обязан быть в ключе.
            parts.Add($"avail{(OnlyAvailable == true ? "1" : "0")}");
            parts.Add($"sort{SortBy}_{(SortDesc ? "desc" : "asc")}");

            return string.Join(":", parts);
        }
    }

    // ✅ TTL — 5 минут
    public TimeSpan? CacheDuration => TimeSpan.FromMinutes(5);
}
