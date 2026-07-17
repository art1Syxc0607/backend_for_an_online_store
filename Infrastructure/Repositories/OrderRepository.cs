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
    //public async Task CreateOrder(User user,
    //    List<OrderItemDto> items, string shippingAddress,
    //    CancellationToken ct)
    //{
    //    var orders = items
    //        .Select(i => new Order(user, shippingAddress)).
    //        ToList();

    //    await _dpcontext.Orders.AddRangeAsync(orders);
    //}
    //Task CreateOrders(int userId, List<OrderItemDto> items);

}
