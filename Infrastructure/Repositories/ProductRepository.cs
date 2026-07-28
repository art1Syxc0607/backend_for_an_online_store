using Application.Enums;
using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Infrastructure.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly AppDbContext _dpContext;

    public ProductRepository(AppDbContext dpContext) => 
        _dpContext = dpContext;

    public async Task<Product?> GetByIdAsync(int id, CancellationToken ct)
    {
        return await _dpContext.Products.FirstOrDefaultAsync(p => p.Id == id, ct);
    }

    public async Task<List<Product>?> GetByIdsAsync(List<int> Ids, CancellationToken ct)
    {
        var result = await _dpContext.Products.Where(p => Ids.Contains(p.Id)).ToListAsync();
        return result;
    }

    public async Task<List<Product>?> GetAllProductsAsync(CancellationToken ct)
    {
        return await _dpContext.Products.ToListAsync(ct);
    }

    public async Task<int> AddProductAsync(Product product, CancellationToken ct)
    {
        await _dpContext.Products.AddAsync(product, ct);

        return product.Id;
    }

    public Task UpdateProductAsync(Product product, CancellationToken ct)
    {
        _dpContext.Products.Update(product);

        return Task.CompletedTask;

    }

    public Task DeleteProductAsync(Product product, CancellationToken ct)
    {
        _dpContext.Products.Remove(product);

        return Task.CompletedTask;
    }

    public async Task<bool> ProductsExist(List<int> Ids, CancellationToken ct)
    {
        if (Ids == null || Ids.Count == 0)
            return false;

        // Количество существующих ID должно равняться количеству запрошенных
        var count = await _dpContext.Products
            .CountAsync(p => Ids.Contains(p.Id), ct);

        return count == Ids.Count;
    }

    public async Task<bool> ProductExist(int id)
    {
        return await _dpContext.Products.AnyAsync(p =>  id == p.Id);
    }

    public async Task<List<Product>> GetProductsFilter(int? CategoryId, string? SearchText, decimal? PriceLimitMax,
        decimal? PriceLimitMin, bool? OnlyAvailable, int? pageNumber, int? pageSize, SortBy? sortBy = SortBy.Name, bool SortDesc = true)
    {
        var search = _dpContext.Products.Include(p => p.Reviews)
            .WhereIf(CategoryId != null, p => p.CategoryId == CategoryId)
            .WhereIf(SearchText != null, p => p.Name.Contains(SearchText) || p.Description.Contains(SearchText))
            .WhereIf(PriceLimitMin != null, p => p.Price >= PriceLimitMin)
            .WhereIf(PriceLimitMax != null, p => p.Price <= PriceLimitMax)
            .WhereIf(OnlyAvailable != null, p => p.AvailableQuantity != 0);

        var sortedQuery = search.ApplySorting(sortBy, SortDesc);

        // Pagination
        var paginatedproducts = pageNumber != null && pageSize != null ? sortedQuery
            .Pagination(pageNumber.Value, pageSize.Value) : sortedQuery;



        return await paginatedproducts.ToListAsync();
    }

}

