using Application.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Commands.Order;

public class ReceiveOrderCommandHandler : IRequestHandler<ReceiveOrderCommand>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;


    public ReceiveOrderCommandHandler(IOrderRepository orderRepository, IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(ReceiveOrderCommand command, CancellationToken ct)
    {
        var order = await _orderRepository.GetOrder(command.OrderId, ct);

        if (order == null) throw new Exception("No such order");
        if (order.UserId != command.UserId) throw new Exception("This order doesn't belong to the user");

        order.ReceivedByUser();

        await _unitOfWork.SaveChangesAsync();
    }
}
