using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.DTOs.Order;
using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

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

    public async Task<bool> IfBuyThisProduct(int userId, int productId, CancellationToken ct = default)
    {
        var orders = await GetAllAsync(userId, ct);

        var result = orders.Any(order => order.Status == Domain.Enums.OrderStatus.Delivered && 
        order.Items.Any(oi => oi.ProductId == productId));

        return result;
    }
}
