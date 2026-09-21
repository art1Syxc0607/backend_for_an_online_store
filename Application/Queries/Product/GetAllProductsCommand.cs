using Application.DTOs.Product;
using Application.Interfaces.Caching;
using MediatR;

namespace Application.Queries.Product;

public class GetAllProductsCommand : IRequest<List<ProductResponseDto>>, ICacheableQuery
{
    public string CacheKey
    {
        get
        {
            return CacheKeys.ProductsPrefix;
        }
    }

    // ✅ TTL — 5 минут
    public TimeSpan? CacheDuration => TimeSpan.FromMinutes(5);
}
