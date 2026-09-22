using Application.DTOs.Review;
using Application.Interfaces.Caching;
using Domain.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Queries.Review;

public class GetProductReviewsQuery : IRequest<List<ReviewResponseDto>>, ICacheableQuery
{
    // ═══════════════════════════════════════════
    // Обязательные
    // ═══════════════════════════════════════════

    public int ProductId { get; init; }

    // ═══════════════════════════════════════════
    // Фильтры
    // ═══════════════════════════════════════════

    /// <summary>
    /// Точный рейтинг (1-5). null = все.
    /// </summary>
    public int? Rating { get; init; }

    /// <summary>
    /// Минимальный рейтинг (для "4+" звёзд).
    /// </summary>
    public int? MinRating { get; init; }

    /// <summary>
    /// Есть ли медиа (фото/видео).
    /// null = все, true = только с медиа, false = только без медиа.
    /// </summary>
    public bool? HasMedia { get; init; }

    /// <summary>
    /// Только с подтверждённой покупкой.
    /// </summary>
    public bool? IsVerifiedPurchase { get; init; }

    /// <summary>
    /// Фильтр по дате (от).
    /// </summary>
    public DateTime? FromDate { get; init; }

    /// <summary>
    /// Фильтр по дате (до).
    /// </summary>
    public DateTime? ToDate { get; init; }

    /// <summary>
    /// Только с ответом администратора.
    /// </summary>
    public bool? HasAdminResponse { get; init; }

    // ═══════════════════════════════════════════
    // Сортировка
    // ═══════════════════════════════════════════

    public ReviewSortBy SortBy { get; init; } = ReviewSortBy.DateOfCreation;
    public bool Descending { get; init; } = true;

    // ═══════════════════════════════════════════
    // Пагинация
    // ═══════════════════════════════════════════

    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;

    // ═══════════════════════════════════════════
    // Кэширование
    // ═══════════════════════════════════════════

    public string CacheKey
    {
        get
        {
            var parts = new List<string>
            {
                CacheKeys.ReviewsForProduct(ProductId),
                $"p{PageNumber}",
                $"s{PageSize}",
                $"sort{SortBy}_{(Descending ? "d" : "a")}"
            };

            // ✅ Фильтры включаются ТОЛЬКО если заданы
            if (Rating.HasValue) parts.Add($"r{Rating}");
            if (MinRating.HasValue) parts.Add($"minr{MinRating}");
            if (HasMedia.HasValue) parts.Add($"media{(HasMedia.Value ? "1" : "0")}");
            if (IsVerifiedPurchase.HasValue) parts.Add($"verified{(IsVerifiedPurchase.Value ? "1" : "0")}");
            if (HasAdminResponse.HasValue) parts.Add($"resp{(HasAdminResponse.Value ? "1" : "0")}");
            if (FromDate.HasValue) parts.Add($"from{FromDate:yyyyMMdd}");
            if (ToDate.HasValue) parts.Add($"to{ToDate:yyyyMMdd}");

            return string.Join(":", parts);
        }
    }

    // ✅ TTL зависит от наличия фильтров
    public TimeSpan? CacheDuration => GetCacheDuration();

    private TimeSpan GetCacheDuration()
    {
        // Без фильтров — "горячий" запрос, короткий кэш
        if (!Rating.HasValue && !MinRating.HasValue &&
            !HasMedia.HasValue && !IsVerifiedPurchase.HasValue &&
            !FromDate.HasValue && !ToDate.HasValue && !HasAdminResponse.HasValue)
        {
            return TimeSpan.FromMinutes(5);
        }

        // С фильтрами — длиннее (реже запрашивается)
        return TimeSpan.FromMinutes(15);
    }
}
