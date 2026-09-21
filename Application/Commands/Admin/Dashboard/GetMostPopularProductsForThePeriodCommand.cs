using Application.DTOs.Product;
using Application.Enums;
using Application.Interfaces.Caching;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Commands.Admin.Dashboard;

public class GetMostPopularProductsForThePeriodCommand : IRequest<List<PopularProductDto>>, ICacheableQuery
{
    public DateTime LastDayOfThePeriod { get; set; }
    public DateTime FirstDayOfThePeriod { get; set; }
    public int? PageNumber { get; set; } = 1;
    public int? PageSize { get; set; } = 20;

    public string CacheKey => CacheKeys.PopularProducts(
        FirstDayOfThePeriod,
        LastDayOfThePeriod,
        PageNumber.HasValue ? PageNumber.Value : 1,
        PageSize.HasValue ? PageSize.Value : 20
    );

    // ✅ TTL зависит от периода
    public TimeSpan? CacheDuration => GetCacheDuration();

    private TimeSpan GetCacheDuration()
    {
        var now = DateTime.UtcNow;

        // Прошлое — долгий кэш
        if (LastDayOfThePeriod.Date < now.Date)
            return TimeSpan.FromHours(24);

        // Сегодня — короткий кэш
        if (LastDayOfThePeriod.Date == now.Date)
            return TimeSpan.FromMinutes(15);

        // Будущее — не кэшируем
        return TimeSpan.Zero;
    }
}
