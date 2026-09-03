using Application.DTOs.Order;
using Application.Interfaces;
using AutoMapper;
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
    private readonly IMapper _mapper;

    public GetAllOrderOrFilteredHandler(IOrderRepository orderRepository, IMapper mapper)
    {
        _orderRepository = orderRepository;
        _mapper = mapper;
    }

    public async Task<List<OrderResponseDto>> Handle(GetAllOrderOrFilteredCommand command, 
        CancellationToken ct)
    {
        var orders = await _orderRepository.GetOrdersFilterAsync(command, ct);

        //var result = orders.Select(o => new OrderResponseDto // before
        //{
        //    OrderId = o.Id,
        //    Items = o.Items.Select(ot => new OrderItemDto
        //    {
        //        ProductId = ot.ProductId,
        //        Quantity = ot.Quantity,
        //        PriceAtPurchase = ot.PriceAtPurchase,
        //        ProductNameAtPurchase = ot.ProductNameAtPurchase
        //    }).ToList(),
        //    UserId = o.UserId,
        //    TotalAmount = o.TotalAmount,
        //    ShippingAddress = o.ShippingAddress,
        //    Status = o.Status,

        //    // info 
        //    CreatedAt = o.CreatedAt,
        //    PaidAt = o.PaidAt,
        //    ShippedAt = o.ShippedAt,
        //    DeliveredAt = o.DeliveredAt,
        //    ReceivedAt = o.ReceivedAt,
        //    CancelledAt = o.CancelledAt
        //}).OrderByDescending(orDto => orDto.CreatedAt).ToList();

        // Маппим через AutoMapper
        var result = _mapper.Map<List<OrderResponseDto>>(orders);

        return result;

    }
}
