// Application/Commands/Product/GetMostPopularProductsForThePeriodCommand.cs
using Application.Common;
using Application.Common.Caching;
using Application.DTOs.Product;
using Application.Interfaces.Caching;
using MediatR;

namespace Application.Commands.Product;

public class GetMostPopularProductsForThePeriodForUserCommand
    : IRequest<PagedResult<PopularProductDto>>, ICacheableQuery
{
    public DateTime FirstDayOfThePeriod { get; init; }
    public DateTime LastDayOfThePeriod { get; init; }
    public int? PageNumber { get; init; } = 1;
    public int? PageSize { get; init; } = 20;

    private int NormalizedPageNumber => Math.Max(1, PageNumber ?? 1);
    private int NormalizedPageSize => Math.Clamp(PageSize ?? 20, 1, 50);

    public string CacheKey
    {
        get
        {
            return $"{CacheKeys.PopularProductsPrefix}" +
                   $"{FirstDayOfThePeriod:yyyyMMdd}_" +
                   $"{LastDayOfThePeriod:yyyyMMdd}_" +
                   $"p{NormalizedPageNumber}_" +
                   $"s{NormalizedPageSize}";
        }
    }

    public TimeSpan? CacheDuration => GetCacheTtl();

    private TimeSpan GetCacheTtl()
    {
        var now = DateTime.UtcNow;

        if (LastDayOfThePeriod.Date < now.Date)
            return TimeSpan.FromHours(24);

        if (LastDayOfThePeriod.Date == now.Date)
            return TimeSpan.FromMinutes(15);

        return TimeSpan.Zero;
    }
}