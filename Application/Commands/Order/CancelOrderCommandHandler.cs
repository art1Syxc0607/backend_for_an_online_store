using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Interfaces;
using Domain.Exceptions;
using MediatR;

namespace Application.Commands.Order;

public class CancelOrderCommandHandler : IRequestHandler<CancelOrderCommand>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CancelOrderCommandHandler(
        IOrderRepository orderRepository,
        IUnitOfWork unitOfWork, IUserRepository userRepository)
    {
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(CancelOrderCommand command,
        CancellationToken ct)
    {
        var order = await _orderRepository.GetOrder(command.OrderId, ct);
        if (order == null)
            throw new DomainException("Order not found");

        if (order.UserId != command.UserId)
            throw new DomainException("This order doesn't belong to this user");

        order.Cancel();

        await _orderRepository.UpdateOrder(order, ct);
        await _unitOfWork.SaveChangesAsync(ct);

    }
}
