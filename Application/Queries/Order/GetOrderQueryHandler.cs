using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.DTOs.Order;
using Application.Interfaces;
using Domain.Entities;
using Domain.Exceptions;
using MediatR;

namespace Application.Queries.Order;

public class GetOrderQueryHandler : IRequestHandler<GetOrderQuery, OrderResponseDto>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUserRepository _userRepository;

    public GetOrderQueryHandler(
        IOrderRepository orderRepository,
        IUserRepository userRepository)
    {
        _orderRepository = orderRepository;
        _userRepository = userRepository;
    }

    public async Task<OrderResponseDto> Handle(
        GetOrderQuery query, CancellationToken ct)
    {
        var order = await _orderRepository.GetOrder(query.UserId, ct);
        if (order == null) throw new DomainException("No such order");

        var result = new OrderResponseDto
        {
            Id = order.Id,
            UserId = order.UserId,
            Items = order.Items.Select(i => new OrderItemDto
            (
                i.ProductId, i.Quantity, i.PriceAtPurchase           
            )).ToList(),
            TotalAmount = order.TotalAmount,
            ShippingAddress = order.ShippingAddress,
            Status = order.Status,
            CreatedAt = order.CreatedAt,
            PaidAt = order.PaidAt,
        };

        return result;
    }
}
