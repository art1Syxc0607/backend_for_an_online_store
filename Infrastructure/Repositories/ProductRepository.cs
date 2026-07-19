using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

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

    public async Task<bool> ProductsExist(List<int> Ids, CancellationToken ct)
    {
        if (Ids == null || Ids.Count == 0)
            return false;

        // Количество существующих ID должно равняться количеству запрошенных
        var count = await _dpContext.Products
            .CountAsync(p => Ids.Contains(p.Id), ct);

        return count == Ids.Count;
    }
}
