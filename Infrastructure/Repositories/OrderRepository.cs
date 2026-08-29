using Application.Commands.Admin.Order;
using Application.DTOs.Admin.Order;
using Application.DTOs.Order;
using Application.DTOs.Product;
using Application.Enums;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Infrastructure.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly AppDbContext _dpContext;

    public OrderRepository(AppDbContext dpcontext)
    {
        _dpContext = dpcontext;
    }

    public async Task<Order?> GetOrder(int id, CancellationToken ct = default)
    {
        var result = await _dpContext.Orders
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)  // ← ДОБАВИТЬ! Загружаем продукты
            .FirstOrDefaultAsync(o => o.Id == id);

        return result;
    }

    public async Task<List<Order>> GetAllAsync(int userId, 
        CancellationToken ct = default)
    {
        var result = await _dpContext.Orders.
            Include(o => o.Items).
            Where(
            o => o.UserId == userId).ToListAsync(ct);

        return result;


    }
    public async Task CreateOrder(Order order,
        CancellationToken ct = default)
    {
        await _dpContext.Orders.AddAsync(order);
    }

    public async Task UpdateOrder(Order order, CancellationToken ct = default)
    {
        _dpContext.Orders.Update(order);
        await Task.CompletedTask;
    }

    public async Task<bool> HasProductInOrdersAsync(int productId, CancellationToken ct = default)
    {
        //Сейчас ты проверяешь, есть ли любой заказ с этим товаром.Но если заказ отменен — товар физически не 
        //был продан, и его можно удалить.

        return await _dpContext.Orders
            .AnyAsync(o =>
                o.Status != OrderStatus.Cancelled &&
                o.Items.Any(i => i.ProductId == productId),
                ct);
    }

    public async Task<bool> HasUserPurchasedProductAsync(int userId, int productId, 
        CancellationToken ct = default)
    {

        var result = await _dpContext.Orders
        .AnyAsync(o => o.UserId == userId
                       && o.Status == OrderStatus.Received
                       && o.Items.Any(i => i.ProductId == productId), ct);

        return result;
    }

    //Admin
    //public async Task<List<PurchasesProductForPeriodDto>> MostPopularProductsForThePeriod(DateSpan period,
    //    CancellationToken ct = default)
    //{

    //}

    //public async Task<int> GetNumberOfNewOrdersAsync(DateSpan Span, CancellationToken ct = default)
    //{
    //    return await _dpContext.Orders
    //        .CountAsync(o => IsWithinDateSpan(o.CreatedAt, DateTime.Today, Span));// так нельзя в Ef Core
    //}

    //public async Task<decimal> GetRevenueForThePeriodAsync(DateTime LastDayOfThePriod, DateSpan Span, 
    //    CancellationToken ct = default)
    //{
    //    var result = await _dpContext.Orders.Where(o => o.CreatedAt < LastDayOfThePriod &&
    //        IsWithinDateSpan(o.CreatedAt, LastDayOfThePriod, Span)).SumAsync(o => o.TotalAmount); // так нельзя в Ef Core

    //    return result;
    //}


    //public async Task<List<Order>> GetOrdersFilterAsync(GetAllOrderOrFilteredCommand command,
    //    CancellationToken ct)
    //{
    //    var filteredsearch = _dpContext.Orders.Include(o => o.Items)
    //        .WhereIf(command.Status.HasValue, o => o.Status == command.Status)
    //        .WhereIf(command.Date.HasValue && command.DateSpan.HasValue, o =>
    //            IsWithinDateSpan(o.CreatedAt, command.Date.Value, command.DateSpan.Value)) // так нельзя в Ef Core
    //        .WhereIf(command.UserId.HasValue, o => o.UserId == command.UserId);

    //    var sortedQuery = filteredsearch.ApplyOrderSorting(
    //        command.OrderSortBy,
    //        command.SortDesc ?? true
    //    );

    //    // Pagination
    //    var paginatedorders = command.PageNumber.HasValue && command.PageSize.HasValue ? sortedQuery
    //        .Pagination(command.PageNumber.Value, command.PageSize.Value) : sortedQuery;


    //    return await paginatedorders.ToListAsync(ct);
    //}

    //Admin, Order
    public async Task<List<Order>> GetOrdersFilterAsync(GetAllOrderOrFilteredCommand command, CancellationToken ct)
    {
        var query = _dpContext.Orders.Include(o => o.Items)
            .WhereIf(command.Status.HasValue, o => o.Status == command.Status)
            .WhereIf(command.UserId.HasValue, o => o.UserId == command.UserId);


        // вместо
        //.WhereIf(command.Date.HasValue && command.DateSpan.HasValue, o =>
    //  IsWithinDateSpan(o.CreatedAt, command.Date.Value, command.DateSpan.Value)) // так нельзя в Ef Core
        if (command.Date.HasValue && command.DateSpan.HasValue)
        {
            var referenceDate = command.Date.Value.Date;
            var maxDaysDiff = GetDateSpan(command.DateSpan.Value, referenceDate);

            //query = query.Where(o =>
            //    EF.Functions.DateDiffDay(o.CreatedAt.Date, referenceDate) <= maxDays);

            // Для SQLite (используем EntityFunctions или вычисляем разницу)
            query = query.Where(o =>
                o.CreatedAt.Date >= referenceDate.Date.AddDays(-maxDaysDiff) &&
                o.CreatedAt.Date <= referenceDate.Date.AddDays(maxDaysDiff)
            );
        }

        var sortedQuery = query.ApplyOrderSorting(command.OrderSortBy, command.SortDesc ?? true);

        var paginatedOrders = command.PageNumber.HasValue && command.PageSize.HasValue
            ? sortedQuery.Pagination(command.PageNumber.Value, command.PageSize.Value)
            : sortedQuery;

        return await paginatedOrders.ToListAsync(ct);
    }

    //Admin, Dashboard
    public async Task<int> GetNumberOfNewOrdersAsync(DateSpan span, CancellationToken ct = default)
    {
        var endDate = DateTime.Today.Date;
        var maxDaysDiff = GetDateSpan(span, endDate);
        var startDate = endDate.AddDays(-maxDaysDiff);


        return await _dpContext.Orders
            .CountAsync(o =>
                o.CreatedAt.Date >= startDate &&
                o.CreatedAt.Date <= endDate);
    }

    public async Task<decimal> GetRevenueForThePeriodAsync(DateTime lastDayOfThePriod, DateSpan span,
        CancellationToken ct = default)
    {
        var endDate = lastDayOfThePriod.Date;
        var maxDaysDiff = GetDateSpan(span, endDate);
        var startDate = endDate.AddDays(-maxDaysDiff);

        var result = await _dpContext.Orders.Where(o => o.CreatedAt <= endDate &&
            o.CreatedAt.Date >= startDate)
            //.SumAsync(o => o.TotalAmount);
            .SumAsync(o => (decimal?)o.TotalAmount) ?? 0;

        return result;
    }

    public async Task<decimal> GetCostOfGoodsSoldAsync(DateTime lastDayOfThePriod, DateSpan span,
        CancellationToken ct = default)
    {
        var endDate = lastDayOfThePriod.Date;
        var maxDaysDiff = GetDateSpan(span, endDate);
        var startDate = endDate.AddDays(-maxDaysDiff);

        return await _dpContext.OrderItems
            .Where(oi => oi.Order.CreatedAt >= startDate && oi.Order.CreatedAt <= endDate)
            .Where(oi => oi.Order.Status != OrderStatus.Cancelled && oi.Order.Status != OrderStatus.Pending) // ← фильтр!
            //.SumAsync(oi => oi.PurchasePriceAtPurchase * oi.Quantity);
            .SumAsync(oi => (decimal?)oi.PurchasePriceAtPurchase * oi.Quantity) ?? 0;
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

    //private int GetMaxDays(DateSpan span)
    //{
    //    return span switch
    //    {
    //        DateSpan.Day => 1,
    //        DateSpan.Week => 7,
    //        DateSpan.HalfOfMonth => 15,
    //        DateSpan.Month => 30,
    //        DateSpan.HalfOfYear => 180,
    //        DateSpan.Year => 365,
    //        _ => 0
    //    };
    //}



    //private (DateTime start, DateTime end) GetDateRange(DateTime referenceDate, DateSpan span)
    //{
    //    return span switch
    //    {
    //        DateSpan.Day => (referenceDate, referenceDate.AddDays(1)),
    //        DateSpan.Week => (referenceDate, referenceDate.AddDays(7)),
    //        DateSpan.HalfOfMonth => (referenceDate, referenceDate.AddDays(15)),
    //        DateSpan.Month => (referenceDate, referenceDate.AddDays(30)),
    //        DateSpan.HalfOfYear => (referenceDate, referenceDate.AddMonths(6)),
    //        DateSpan.Year => (referenceDate, referenceDate.AddYears(1)),
    //        _ => (referenceDate, referenceDate.AddDays(1))
    //    };
    //}

    //private bool IsWithinDateSpan(DateTime orderCreationDate, DateTime referenceDate,
    //    DateSpan span)
    //{
    //    var diff = Math.Abs((orderCreationDate.Date - referenceDate.Date).Days);
    //    var maxDays = GetDateSpan(span, orderCreationDate).Days;
    //    return diff <= maxDays;
    //}
}


//DateTime orderCreationDate, DateTime referenceDate,