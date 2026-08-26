using Application.Interfaces;
using Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Commands.Order;

public class CancelOrderCommandHandler : IRequestHandler<CancelOrderCommand>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ILogger<InitiatePaymentHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public CancelOrderCommandHandler(
        IOrderRepository orderRepository,
        IUnitOfWork unitOfWork, ILogger<InitiatePaymentHandler> logger, 
        IUserRepository userRepository)
    {
        _orderRepository = orderRepository;
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(CancelOrderCommand command,
        CancellationToken ct)
    {
        _logger.LogInformation(
            "Order cancellation started: OrderId {OrderId}, UserId {UserId}",
            command.OrderId,
            command.UserId
        );

        var order = await _orderRepository.GetOrder(command.OrderId, ct);
        if (order == null)
            throw new DomainException("Order not found");

        if (order.UserId != command.UserId)
            throw new DomainException("This order doesn't belong to this user");

        order.Cancel();

        await _orderRepository.UpdateOrder(order, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Order cancelled successfully: OrderId {OrderId}, UserId {UserId}, Status {Status}, Time {Time}",
            order.Id,
            command.UserId,
            order.Status,
            DateTime.UtcNow
        );

    }
}
