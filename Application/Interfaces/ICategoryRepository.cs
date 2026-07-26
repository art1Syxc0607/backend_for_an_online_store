using Application.DTOs.Category;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces;

public interface ICategoryRepository
{
    Task<List<CategoryResponseDto>?> GetAllCategoriesAsync(CancellationToken ct);

    Task<Category> GetByIdAsync(int categoryId, CancellationToken ct);

    Task AddAsync(Category category, CancellationToken ct);

    Task UpdateAsync(Category category, CancellationToken ct);

    Task DeleteAsync(Category category, CancellationToken ct);

    Task<bool> ExistByIdAsync(int categoryId, CancellationToken ct);
}
