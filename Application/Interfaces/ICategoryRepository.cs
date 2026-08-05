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
    Task<List<Category>> GetAllCategoriesAsync(CancellationToken ct = default);

    Task<Category?> GetByIdAsync(int categoryId, CancellationToken ct = default);

    Task<List<Category>> GetByIdsAsync(List<int> ids, CancellationToken ct = default);

    //Task<List<Category>> GetUserCategoriesAsync(int userId, CancellationToken ct = default);

    Task CreateAsync(Category category, CancellationToken ct = default);

    Task UpdateAsync(Category category);

    Task DeleteAsync(Category category);
    Task DeleteAllAsync();

    Task<bool> ExistByIdAsync(int categoryId, CancellationToken ct = default);
}
