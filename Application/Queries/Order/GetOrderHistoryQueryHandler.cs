using Application.DTOs.Order;
using Application.Interfaces;
using Domain.Exceptions;
using MediatR;


namespace Application.Queries.Order;

public class GetOrderHistoryQueryHandler : IRequestHandler
    <GetOrderHistoryQuery, List<OrderResponseDto>>
{
    private readonly IOrderRepository _orderRepository;

    public GetOrderHistoryQueryHandler(
        IOrderRepository orderRepository,
        IUserRepository userRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<List<OrderResponseDto>> Handle(
        GetOrderHistoryQuery query, CancellationToken ct)
    {
        var orders = await _orderRepository.GetAllAsync(query.userId);

        var result = orders.Select(o => new OrderResponseDto
        {
            Id = o.Id,
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
