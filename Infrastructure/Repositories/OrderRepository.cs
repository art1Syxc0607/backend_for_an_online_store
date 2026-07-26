using Application.DTOs.Order;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly AppDbContext _dpcontext;

    public OrderRepository(AppDbContext dpcontext)
    {
        _dpcontext = dpcontext;
    }

    public async Task<Order?> GetOrder(int id, CancellationToken ct)
    {
        var result = await _dpcontext.Orders.
            Include(o => o.Items).
            FirstOrDefaultAsync(o => o.Id == id);

        return result;
    }

    public async Task<List<Order>> GetAllAsync(int userId, 
        CancellationToken ct)
    {
        var result = await _dpcontext.Orders.
            Include(o => o.Items).
            Where(
            o => o.UserId == userId).ToListAsync(ct);

        return result;


    }
    public async Task CreateOrder(Order order,
        CancellationToken ct)
    {
        await _dpcontext.Orders.AddAsync(order);
    }

    public async Task UpdateOrder(Order order, CancellationToken ct = default)
    {
        _dpcontext.Orders.Update(order);
        await Task.CompletedTask;
    }

    public async Task<bool> HasProductInOrdersAsync(int productId, CancellationToken ct)
    {
        //Сейчас ты проверяешь, есть ли любой заказ с этим товаром.Но если заказ отменен — товар физически не 
        //был продан, и его можно удалить.

        return await _dpcontext.Orders
            .AnyAsync(o =>
                o.Status != OrderStatus.Cancelled &&
                o.Items.Any(i => i.ProductId == productId),
                ct);
    }

    public async Task<bool> HasUserPurchasedProductAsync(int userId, int productId, CancellationToken ct = default)
    {
        //var orders = await GetAllAsync(userId, ct);

        //var result = orders.Any(order => order.Status == Domain.Enums.OrderStatus.Delivered && 
        //order.Items.Any(oi => oi.ProductId == productId));

        var result = await _dpcontext.Orders
        .AnyAsync(o => o.UserId == userId
                       && o.Status == OrderStatus.Delivered
                       && o.Items.Any(i => i.ProductId == productId), ct);

        return result;
    }
}
