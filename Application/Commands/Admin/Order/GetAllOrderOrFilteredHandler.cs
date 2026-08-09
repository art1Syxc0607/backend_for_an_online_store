using Application.DTOs.Order;
using Application.Interfaces;
using Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Commands.Admin.Order;

public class GetAllOrderOrFilteredHandler : IRequestHandler<GetAllOrderOrFilteredCommand, 
    List<OrderResponseDto>>
{
    private readonly IOrderRepository _orderRepository;

    public GetAllOrderOrFilteredHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<List<OrderResponseDto>> Handle(GetAllOrderOrFilteredCommand command, 
        CancellationToken ct)
    {
        var orders = await _orderRepository.GetOrdersFilterAsync(command, ct);

        var result = orders.Select(o => new OrderResponseDto
        {
            Id = o.Id,
            Items = o.Items.Select(ot => new OrderItemDto(
                ot.ProductId,
                ot.Quantity,
                ot.PriceAtPurchase,
                ot.ProductNameAtPurchase
            )).ToList(),
            UserId = o.UserId,
            TotalAmount = o.TotalAmount,
            ShippingAddress = o.ShippingAddress,
            Status = o.Status,
            CreatedAt = o.CreatedAt,
            PaidAt = o.PaidAt,
        }).ToList();

        return result;

    }
}
