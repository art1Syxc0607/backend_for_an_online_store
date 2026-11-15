using Application.DTOs.Category;
using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly AppDbContext _dbContext;

    public CategoryRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<Category>> GetAllCategoriesAsync(CancellationToken ct = default)
    {
        return await _dbContext.Categories.ToListAsync(ct);
    }

    public async Task<Category?> GetByIdAsync(int categoryId, CancellationToken ct = default)
    {
        return await _dbContext.Categories.FirstOrDefaultAsync(c => c.Id == categoryId, ct);
    }

    public async Task<List<Category>> GetByIdsAsync(List<int> ids, CancellationToken ct = default)
    {
        return await _dbContext.Categories.Where(c => ids.Contains(c.Id)).ToListAsync(ct);
    }

    public async Task CreateAsync(Category category, CancellationToken ct = default)
    {
        await _dbContext.Categories.AddAsync(category, ct);
    }

    public async Task UpdateAsync(Category category)
    {
        _dbContext.Categories.Update(category);

        await Task.CompletedTask;
    }


    public async Task DeleteAsync(Category category)
    {
        _dbContext.Categories.Remove(category);

        await Task.CompletedTask;
    }

    public async Task DeleteAllAsync()
    {
       await _dbContext.Categories.ExecuteDeleteAsync();
    }

    public async Task<bool> ExistByIdAsync(int categoryId, CancellationToken ct = default)
    {
        return await _dbContext.Categories.AnyAsync(c => c.Id == categoryId, ct);
    }

    public async Task<bool> ExistsByNameAsync(
        string name,
        int? excludeCategoryId = null,
        CancellationToken ct = default)
    {
        var normalized = name.Trim().ToLowerInvariant();

        // Compares lower("Name") so "Обувь" and "обувь" are the same category.
        return await _dbContext.Categories.AnyAsync(
            c => (excludeCategoryId == null || c.Id != excludeCategoryId)
                 && c.Name.ToLower() == normalized,
            ct);
    }
}
