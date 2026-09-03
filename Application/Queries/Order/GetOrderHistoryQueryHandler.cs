using Application.DTOs.Order;
using Application.Interfaces;
using AutoMapper;
using Domain.Exceptions;
using MediatR;


namespace Application.Queries.Order;

public class GetOrderHistoryQueryHandler : IRequestHandler
    <GetOrderHistoryQuery, List<OrderResponseDto>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public GetOrderHistoryQueryHandler(
        IOrderRepository orderRepository, IUserRepository userRepository, IMapper mapper)
    {
        _orderRepository = orderRepository;
        _userRepository = userRepository;
        _mapper = mapper;
    }

    public async Task<List<OrderResponseDto>> Handle(
        GetOrderHistoryQuery query, CancellationToken ct)
    {
        var user = await _userRepository.GetByIdAsync(query.UserId);
        if (user == null) throw new DomainException("User not found.");

        var orders = await _orderRepository.GetAllAsync(query, ct);

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
