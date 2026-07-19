using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.DTOs.Order;
using Domain.Entities;

namespace Application.Interfaces;

public interface IOrderRepository
{
    Task<Order?> GetOrder(int id, CancellationToken ct);
    Task<List<Order>> GetAllAsync(int userId,
        CancellationToken ct);
    //Task CreateOrder(User user, 
    //    List<OrderItemDto> items, string shippingAddress,
    //    CancellationToken ct);

    Task CreateOrder(Order order,
    CancellationToken ct);

    Task UpdateOrder(Order order, CancellationToken ct);

    Task<bool> IfBuyThisProduct(int userId, int productId, CancellationToken ct = default);
    //Task CreateOrders(int userId, List<OrderItemDto> items);
}
