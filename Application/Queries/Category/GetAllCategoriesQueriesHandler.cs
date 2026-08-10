using Application.DTOs.Category;
using Application.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Queries.Category;

public class GetAllCategoriesQueriesHandler : IRequestHandler<GetAllCategoriesQueries, List<CategoryResponseDto>>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly ICacheService _cacheService;

    private const string CacheKey = "categories:all";

    public GetAllCategoriesQueriesHandler(ICategoryRepository categoryRepository, 
        ICacheService cacheService)
    {
        _categoryRepository = categoryRepository;
        _cacheService = cacheService;
    }

    public async Task<List<CategoryResponseDto>> Handle(GetAllCategoriesQueries request, CancellationToken ct)
    {
        // 1. Пытаемся получить из кэша
        var cached = await _cacheService.GetAsync<List<CategoryResponseDto>>(CacheKey);
        if (cached != null)
            return cached;

        // 2. Если нет — загружаем из БД
        var categories = await _categoryRepository.GetAllCategoriesAsync(ct);
        var result = categories.Select(c => new CategoryResponseDto
        {
            Id = c.Id,
            Name = c.Name,
            Description = c.Description,
            CreatedAt = c.CreatedAt
        }).ToList();

        // 3. Сохраняем в кэш на 1 час
        await _cacheService.SetAsync(CacheKey, result, TimeSpan.FromHours(1));

        return result;
    }
}
