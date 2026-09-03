using Application.DTOs.Order;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using Domain.Exceptions;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Queries.Order;

public class GetOrderQueryHandler : IRequestHandler<GetOrderQuery, OrderResponseDto>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public GetOrderQueryHandler(
        IOrderRepository orderRepository,
        IUserRepository userRepository, IMapper mapper)
    {
        _orderRepository = orderRepository;
        _userRepository = userRepository;
        _mapper = mapper;
    }

    public async Task<OrderResponseDto> Handle(
        GetOrderQuery query, CancellationToken ct)
    {
        var order = await _orderRepository.GetOrder(query.OrderId, ct);
        if (order == null) throw new DomainException("No such order");
        if (order.UserId != query.UserId) throw new DomainException("This order doesn't belong to the user");

        //var result = new OrderResponseDto
        //{
        //    OrderId = order.Id,
        //    UserId = order.UserId,
        //    Items = order.Items.Select(ot => new OrderItemDto
        //    {
        //        ProductId = ot.ProductId,
        //        Quantity = ot.Quantity,
        //        PriceAtPurchase = ot.PriceAtPurchase,
        //        ProductNameAtPurchase = ot.ProductNameAtPurchase
        //    }).ToList(),
        //    TotalAmount = order.TotalAmount,
        //    ShippingAddress = order.ShippingAddress,
        //    Status = order.Status,
        //    CreatedAt = order.CreatedAt,
        //    PaidAt = order.PaidAt,
        //};

        // Маппим через AutoMapper
        var result = _mapper.Map<OrderResponseDto>(order);

        return result;
    }
}
