using Application.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Commands.Order;

public class InitiateShipmentCommandHandler : IRequestHandler<InitiateShipmentCommand>
{
    //private readonly IProductRepository _productRepository;

    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;


    public InitiateShipmentCommandHandler(IOrderRepository orderRepository, IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(InitiateShipmentCommand command, CancellationToken ct)
    {
        var order = await _orderRepository.GetOrder(command.OrderId, ct);

        if (order == null) throw new Exception("No such order");

        order.Ship();

        await _unitOfWork.SaveChangesAsync();
    }
}
