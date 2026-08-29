using Application.DTOs.Order;
using Application.Interfaces;
using Domain.Exceptions;
using MediatR;


namespace Application.Queries.Order;

public class GetOrderHistoryQueryHandler : IRequestHandler
    <GetOrderHistoryQuery, List<OrderResponseDto>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUserRepository _userRepository;

    public GetOrderHistoryQueryHandler(
        IOrderRepository orderRepository, IUserRepository userRepository)
    {
        _orderRepository = orderRepository;
        _userRepository = userRepository;
    }

    public async Task<List<OrderResponseDto>> Handle(
        GetOrderHistoryQuery query, CancellationToken ct)
    {
        var user = await _userRepository.GetByIdAsync(query.userId);
        if (user == null) throw new DomainException("User not found.");

        var orders = await _orderRepository.GetAllAsync(query.userId, ct);

        var result = orders.Select(o => new OrderResponseDto
        {
            OrderId = o.Id,
            Items = o.Items.Select(ot => new OrderItemDto
            {
                ProductId = ot.ProductId,
                Quantity = ot.Quantity,
                PriceAtPurchase = ot.PriceAtPurchase,
                ProductNameAtPurchase = ot.ProductNameAtPurchase
            }).ToList(),
            UserId = o.UserId,
            TotalAmount = o.TotalAmount,
            ShippingAddress = o.ShippingAddress,
            Status = o.Status,
            CreatedAt = o.CreatedAt,
            PaidAt = o.PaidAt,
        }).OrderByDescending(orDto => orDto.CreatedAt).ToList();

        return result;
    }
}
