using Application.DTOs.Product;
using Application.Enums;
using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace Infrastructure.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly AppDbContext _dpContext;

    public ProductRepository(AppDbContext dpContext) => 
        _dpContext = dpContext;

    public async Task<Product?> GetByIdAsync(int id, CancellationToken ct = default )
    {
        return await _dpContext.Products.FirstOrDefaultAsync(p => p.Id == id, ct);
    }

    public async Task<List<Product>?> GetByIdsAsync(List<int> Ids, CancellationToken ct = default)
    {
        var result = await _dpContext.Products.Where(p => Ids.Contains(p.Id)).ToListAsync();
        return result;
    }

    public async Task<List<Product>?> GetAllProductsAsync(CancellationToken ct = default)
    {
        return await _dpContext.Products.ToListAsync(ct);
    }

    public async Task<int> AddProductAsync(Product product, CancellationToken ct = default)
    {
        await _dpContext.Products.AddAsync(product, ct);

        return product.Id;
    }

    public Task UpdateProductAsync(Product product, CancellationToken ct = default)
    {
        _dpContext.Products.Update(product);

        return Task.CompletedTask;

    }

    public Task DeleteProductAsync(Product product, CancellationToken ct = default)
    {
        _dpContext.Products.Remove(product);

        return Task.CompletedTask;
    }

    public async Task<bool> ProductsExist(List<int> Ids, CancellationToken ct = default)
    {
        if (Ids == null || Ids.Count == 0)
            return false;

        // Количество существующих ID должно равняться количеству запрошенных
        var count = await _dpContext.Products
            .CountAsync(p => Ids.Contains(p.Id), ct);

        return count == Ids.Count;
    }

    public async Task<bool> ProductExist(int id, CancellationToken ct = default)
    {
        return await _dpContext.Products.AnyAsync(p =>  id == p.Id);
    }


    public async Task<List<Product>> GetProductsFilter(int? CategoryId, string? SearchText, decimal? PriceLimitMax,
        decimal? PriceLimitMin, bool? OnlyAvailable, int? pageNumber, int? pageSize, 
        SortProductBy? sortBy = SortProductBy.Name, bool SortDesc = true, CancellationToken ct = default)
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

    //admin

    public async Task<List<PopularProductDto>> GetMostPopularProductsForThePeriod(
    DateSpan period,
    DateTime lastDayOfThePriod,
    CancellationToken ct = default)
    {
        var endDate = lastDayOfThePriod.Date;
        var maxDaysDiff = GetDateSpan(period, endDate);
        var startDate = endDate.AddDays(-maxDaysDiff);

        // ✅ ОДИН SQL-запрос с группировкой
        var query = await _dpContext.OrderItems
            .Include(oi => oi.Product)  // ← загружаем Product
            .Where(oi => oi.Order.CreatedAt.Date >= startDate &&
                         oi.Order.CreatedAt.Date <= endDate)
            .GroupBy(oi => oi.ProductId)
            .Select(g => new PopularProductDto
            {
                ProductId = g.Key,
                Name = g.First().Product.Name,  // ← теперь работает!
                Description = g.First().Product.Description,
                Price = g.First().Product.Price,
                StockQuantity = g.First().Product.StockQuantity,
                ReservedQuantity = g.First().Product.ReservedQuantity,
                TotalPurchases = g.Sum(oi => oi.Quantity),
                PresenceInOrders = g.Count(),
                ImageUrls = g.First().Product.ImageUrls.ToList(),
                VideoUrls = g.First().Product.VideoUrls.ToList(),
                CategoryId = g.First().Product.CategoryId,
                CreatedAt = g.First().Product.CreatedAt,
                UpdatedAt = g.First().Product.UpdatedAt
            })
            .OrderByDescending(p => p.PresenceInOrders)
            .ToListAsync(ct);

        return query;
    }

    private int GetDateSpan(DateSpan span, DateTime referenceDate) // учитывает что за тип года, сколь
        // ко дней в месяце и тд
    {

        return span switch
        {
            DateSpan.Day => TimeSpan.FromDays(1).Days,
            DateSpan.Week => TimeSpan.FromDays(7).Days,
            DateSpan.HalfOfMonth => TimeSpan.FromDays(15).Days,
            DateSpan.Month => (referenceDate.AddMonths(1) - referenceDate).Days,
            DateSpan.HalfOfYear => (referenceDate.AddMonths(6) - referenceDate).Days,
            DateSpan.Year => (referenceDate.AddYears(1) - referenceDate).Days,
            _ => TimeSpan.Zero.Days
        };
    }

    public async Task<List<Product>> GetLowStockProductsAsync(
        int limit,
        bool includeReserved = true,
        int? categoryId = null,
        string? search = null,
        CancellationToken ct = default)
    {
        var query = _dpContext.Products
            .Include(p => p.Category)
            .Include(p => p.OrderItems)
            .AsQueryable();

        // ✅ Фильтр по категории
        if (categoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == categoryId.Value);
        }

        // ✅ Поиск по названию
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(p => p.Name.Contains(search));
        }

        // ✅ Фильтр по количеству на складе
        if (includeReserved)
        {
            // Учитываем резерв: показываем товары, у которых доступно <= limit
            query = query.Where(p => p.AvailableQuantity <= limit);
        }
        else
        {
            // Только физический остаток
            query = query.Where(p => p.StockQuantity <= limit);
        }

        // ✅ Сортировка: сначала те, которых осталось меньше всего
        return await query
            .OrderBy(p => includeReserved ? p.AvailableQuantity : p.StockQuantity)
            .ThenBy(p => p.Name)
            .ToListAsync(ct);
    }

}

