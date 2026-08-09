using Application.Commands.Admin.Order;
using Application.DTOs.Order;
using Application.Enums;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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


    // Admin
    Task<int> GetNumberOfNewOrdersAsync(DateSpan Span, CancellationToken ct = default);

    Task<decimal> GetRevenueForThePeriodAsync(DateTime LastDayOfThePriod, DateSpan Span, CancellationToken ct = default);
    Task<decimal> GetCostOfGoodsSoldAsync(DateTime lastDayOfThePriod, DateSpan span,
        CancellationToken ct = default);
    Task<List<Order>> GetOrdersFilterAsync(GetAllOrderOrFilteredCommand command,
        CancellationToken ct = default);
}
