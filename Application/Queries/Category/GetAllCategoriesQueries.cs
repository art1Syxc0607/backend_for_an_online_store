using Application.DTOs.Category;
using Application.Interfaces.Caching;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Queries.Category;

public class GetAllCategoriesQueries : IRequest<List<CategoryResponseDto>>, ICacheableQuery
{
    public string CacheKey => CacheKeys.Categories();

    // ✅ TTL — 24 часа (редко меняются)
    public TimeSpan? CacheDuration => TimeSpan.FromHours(24);
}
