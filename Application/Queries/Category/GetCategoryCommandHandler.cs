using Application.DTOs.Category;
using Application.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Queries.Category;

public class GetCategoryCommandHandler : IRequestHandler<GetCategoryCommand, CategoryResponseDto>
{
    private readonly ICategoryRepository _categoryRepository;

    public GetCategoryCommandHandler(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<CategoryResponseDto> Handle(GetCategoryCommand command, CancellationToken ct)
    {
        var category = await _categoryRepository.GetByIdAsync(command.Id, ct);

        if (category == null) throw new Exception("Error");

        var result = new CategoryResponseDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            CreatedAt = category.CreatedAt,
        };

        return result;
    }
}
