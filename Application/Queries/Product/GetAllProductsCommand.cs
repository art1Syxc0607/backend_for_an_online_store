using Application.DTOs.Product;
using Application.Interfaces.Caching;
using Application.Common;
using MediatR;

namespace Application.Queries.Product;

public class GetAllProductsCommand : IRequest<PagedResult<ProductResponseDto>>, ICacheableQuery
{
    public string CacheKey
    {
        get
        {
            // ✅ Ключ обязан учитывать пагинацию, иначе все страницы
            // делят одну запись кэша.
            var pageNumber = Math.Max(1, PageNumber);
            var pageSize = Math.Clamp(PageSize, 1, 50);

            return CacheKeys.ProductsList(pageNumber, pageSize);
        }
    }

    // ✅ TTL — 5 минут
    public TimeSpan? CacheDuration => TimeSpan.FromMinutes(5);


    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}
