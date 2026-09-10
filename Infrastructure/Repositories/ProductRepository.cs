using Application.Commands.Admin.Dashboard;
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
        return await _dpContext.Products
            .Include(p => p.OrderItems)
            .Include(p => p.Reviews)
            .ToListAsync(ct);
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
        var search = _dpContext.Products
            .Include(p => p.OrderItems)
            .Include(p => p.Reviews)
            .WhereIf(CategoryId != null, p => p.CategoryId == CategoryId)
            .WhereIf(SearchText != null, p => p.Name.Contains(SearchText) || p.Description.Contains(SearchText))
            .WhereIf(PriceLimitMin != null, p => p.Price >= PriceLimitMin)
            .WhereIf(PriceLimitMax != null, p => p.Price <= PriceLimitMax)
            .WhereIf(OnlyAvailable != null, p => p.StockQuantity - p.ReservedQuantity != 0);

        var sortedQuery = search.ApplySorting(sortBy, SortDesc);

        // Pagination
        var paginatedproducts = pageNumber != null && pageSize != null ? sortedQuery
            .Pagination(pageNumber.Value, pageSize.Value) : sortedQuery;



        return await paginatedproducts.ToListAsync();
    }

    //admin

    public async Task<List<PopularProductDto>> GetMostPopularProductsForThePeriod(
        GetMostPopularProductsForThePeriodCommand command, CancellationToken ct = default)
    {
        var endDate = command.LastDayOfThePriod.Date;
        var startDate = command.FirstDayOfThePriod.Date;
        var pageNumber = command.PageNumber ?? 1;
        var pageSize = command.PageSize ?? 20;

        // ✅ ОДИН SQL-запрос с группировкой
        //var query = await _dpContext.OrderItems
        //    .Where(oi => oi.Order.CreatedAt.Date >= startDate &&
        //                 oi.Order.CreatedAt.Date <= endDate)
        //    .GroupBy(oi => oi.ProductId)
        //    .Select(gOfOi => new PopularProductDto
        //    {
        //        ProductId = gOfOi.Key,
        //        // Используем проекцию внутри Select
        //        Name = gOfOi.Select(oi => oi.Product.Name).FirstOrDefault(),
        //        Description = gOfOi.Select(oi => oi.Product.Description).FirstOrDefault(),
        //        Price = gOfOi.Select(oi => oi.Product.Price).FirstOrDefault(),
        //        StockQuantity = gOfOi.Select(oi => oi.Product.StockQuantity).FirstOrDefault(),
        //        ReservedQuantity = gOfOi.Select(oi => oi.Product.ReservedQuantity).FirstOrDefault(),
        //        ImageUrls = gOfOi.Select(oi => oi.Product.ImageUrls.ToList()).FirstOrDefault(),
        //        VideoUrls = gOfOi.Select(oi => oi.Product.VideoUrls.ToList()).FirstOrDefault(),
        //        CategoryId = gOfOi.Select(oi => oi.Product.CategoryId).FirstOrDefault(),
        //        CreatedAt = gOfOi.Select(oi => oi.Product.CreatedAt).FirstOrDefault(),
        //        UpdatedAt = gOfOi.Select(oi => oi.Product.UpdatedAt).FirstOrDefault(),

        //        TotalPurchases = gOfOi.Sum(oi => oi.Quantity),
        //        PresenceInOrders = gOfOi.Count()
        //    })
        //    .OrderByDescending(p => p.PresenceInOrders)
        //    .Pagination(pageNumber, pageSize)
        //    .ToListAsync(ct);


        // 1. Получаем агрегированные данные
        var stats = await _dpContext.OrderItems
            .Where(oi => oi.CreatedAt.Date >= startDate &&
                         oi.CreatedAt.Date <= endDate)
            .GroupBy(oi => oi.ProductId)
            .Select(g => new // EF Core как-то сам подгрузить нужные поле в Order
            {
                ProductId = g.Key,
                TotalAmountOfProductInOrders = g.Sum(oi => oi.Quantity),
                PresenceOfProductInOrders = g.Count(),
                // ✅ EF Core подгрузит ТОЛЬКО Order.Status
                AmountOfPendingForThePeriod = g.Count(oi => oi.Order.Status == Domain.Enums.OrderStatus.Pending),
                AmountOfPaidForThePeriod = g.Count(oi => oi.Order.Status == Domain.Enums.OrderStatus.Paid),
                AmountOfShippedForThePeriod = g.Count(oi => oi.Order.Status == Domain.Enums.OrderStatus.Shipped),
                AmountOfDeliveredForThePeriod = g.Count(oi => oi.Order.Status == Domain.Enums.OrderStatus.Delivered),
                AmountOfReceivedForThePeriod = g.Count(oi => oi.Order.Status == Domain.Enums.OrderStatus.Received),
                AmountOfCancelledForThePeriod = g.Count(oi => oi.Order.Status == Domain.Enums.OrderStatus.Cancelled),
            })
            .OrderByDescending(x => x.PresenceOfProductInOrders)
            .Pagination(pageNumber, pageSize)
            .ToListAsync(ct);

        // 2. Загружаем только нужные продукты (по ID)
        var productIds = stats.Select(x => x.ProductId).ToList();
        var products = await _dpContext.Products
            .Where(p => productIds.Contains(p.Id))
            .ToListAsync(ct);

        // 1. Создаём словарь: O(M) — один проход по products
        var productDict = products.ToDictionary(p => p.Id);
        //var statsDict = stats.ToDictionary(s => s.Id);

        // 2. Проекция: O(N) — один проход по stats
        var result = stats.Select(stat =>
        {
            var product = productDict[stat.ProductId]; // ← O(1)
            return new PopularProductDto
            {
                ProductId = stat.ProductId,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                StockQuantity = product.StockQuantity,
                ReservedQuantity = product.ReservedQuantity,
                ImageUrls = product.ImageUrls.ToList() ?? new List<string>(),
                VideoUrls = product.VideoUrls.ToList() ?? new List<string>(),

                // stat of product for the period: startDate - endDate
                AmountOfPendingForThePeriod = stat.AmountOfPendingForThePeriod,
                AmountOfPaidForThePeriod = stat.AmountOfPaidForThePeriod,
                AmountOfShippedForThePeriod = stat.AmountOfShippedForThePeriod,
                AmountOfDeliveredForThePeriod = stat.AmountOfDeliveredForThePeriod,
                AmountOfReceivedForThePeriod = stat.AmountOfReceivedForThePeriod,
                AmountOfCancelledForThePeriod = stat.AmountOfCancelledForThePeriod,

                CategoryId = product.CategoryId,
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt,
                TotalPurchases = stat.TotalAmountOfProductInOrders,
                PresenceInOrders = stat.PresenceOfProductInOrders
            };
        }).ToList();
        // стоит добавить индексы на 
        //на CreatedAt в OrderItems, на ProductId в OrderItems 

        return result;
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
            query = query.Where(p => p.StockQuantity - p.ReservedQuantity <= limit);
        }
        else
        {
            // Только физический остаток
            query = query.Where(p => p.StockQuantity <= limit);
        }

        // ✅ Сортировка: сначала те, которых осталось меньше всего
        return await query
            .OrderBy(p => includeReserved ? p.StockQuantity - p.ReservedQuantity : p.StockQuantity)
            .ThenBy(p => p.Name)
            .ToListAsync(ct);
    }

}

