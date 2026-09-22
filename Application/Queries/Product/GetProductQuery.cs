using Application.DTOs.Product;
using Application.Interfaces.Caching;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Queries.Product;

public class GetProductQuery : IRequest<ProductResponseDto>, ICacheableQuery
{
    public int Id { get; set; }

    public string CacheKey
    {
        get
        {
            return CacheKeys.Product(Id);
        }
    }

    // ✅ TTL — 5 минут
    public TimeSpan? CacheDuration => TimeSpan.FromMinutes(15);
}
