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

    public GetAllCategoriesQueriesHandler(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<List<CategoryResponseDto>> Handle(GetAllCategoriesQueries request, CancellationToken ct)
    {
        var categories = await _categoryRepository.GetAllCategoriesAsync(ct);

        if (categories == null) throw new Exception("Error");

        var result = categories.Select(c => new CategoryResponseDto
        {
            Id = c.Id,
            Name = c.Name,
            Description = c.Description,
            CreatedAt = c.CreatedAt,
        }).ToList();

        return result;
    }
}
