using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Interfaces;
using Domain.Exceptions;
using MediatR;

namespace Application.Commands.Order;

public record CancelOrderCommandHandler : IRequestHandler<CancelOrderCommand>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CancelOrderCommandHandler(
        IOrderRepository orderRepository,
        IUnitOfWork unitOfWork, IUserRepository userRepository)
    {
        _orderRepository = orderRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(CancelOrderCommand command,
        CancellationToken ct)
    {
        var user = await _userRepository.GetByIdAsync(command.UserId,
            ct);
        if (user == null)
            throw new DomainException("No such user");

        var order = await _orderRepository.GetOrder(command.OrderId, ct);
        if (order == null) throw new DomainException("No such order");
        if(order.UserId != user.Id) throw new DomainException("This order doesn't belong to this user");

        order.Cancel();

    }
}
