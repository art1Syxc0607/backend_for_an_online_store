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

public class GetProductsFilterCommand : IRequest<List<ProductResponseDto>>, ICacheableQuery
{
    public string? SearchText { get; init; }
    public int? CategoryId { get; init; }
    public decimal? PriceLimitMax { get; init; }
    public decimal? PriceLimitMin { get; init; }
    public bool? OnlyAvailable { get; init; } = true;
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public SortProductBy SortBy { get; init; } = 0;
    public bool SortDesc { get; init; } = true;
    //public bool? OnlyOutOfUserCart { get; init; } = false;

    // ✅ Кэш-ключ
    public string CacheKey
    {
        get
        {
            var parts = new List<string>
            {
                CacheKeys.ProductsPrefix + "list",
                $"p{PageNumber}",
                $"s{PageSize}"
            };

            if (CategoryId.HasValue) parts.Add($"cat{CategoryId}");
            if (PriceLimitMin.HasValue) parts.Add($"min{PriceLimitMin}");
            if (PriceLimitMax.HasValue) parts.Add($"max{PriceLimitMax}");
            if (!string.IsNullOrWhiteSpace(SearchText)) parts.Add($"q{SearchText}");
            parts.Add($"sort{SortBy}_{(SortDesc ? "desc" : "asc")}");

            return string.Join(":", parts);
        }
    }

    // ✅ TTL — 5 минут
    public TimeSpan? CacheDuration => TimeSpan.FromMinutes(5);
}
