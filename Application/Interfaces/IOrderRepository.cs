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
    Task<Order?> GetOrder(int id, CancellationToken ct = default);
    Task<List<Order>> GetAllAsync(int userId,
        CancellationToken ct = default);
    //Task CreateOrder(User user, 
    //    List<OrderItemDto> items, string shippingAddress,
    //    CancellationToken ct);

    Task CreateOrder(Order order,
    CancellationToken ct = default);

    Task UpdateOrder(Order order, CancellationToken ct = default);

    Task<bool> HasProductInOrdersAsync(int id, CancellationToken ct = default);

    Task<bool> HasUserPurchasedProductAsync(int userId, int productId, 
        CancellationToken ct = default);
    //Task CreateOrders(int userId, List<OrderItemDto> items);
}
