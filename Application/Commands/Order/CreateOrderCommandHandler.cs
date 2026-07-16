using MediatR;
using Application.Interfaces;
using Domain.Exceptions;

namespace Application.Commands.Order;

public class CreateOrderCommandHandler : IRequestHandler
    <CreateOrderCommand>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateOrderCommandHandler(
        IOrderRepository orderRepository,
        IUnitOfWork unitOfWork, IUserRepository userRepository)
    {
        _orderRepository = orderRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(CreateOrderCommand command,
        CancellationToken ct)
    {
        var user = await _userRepository.GetByIdAsync(command.UserId,
            ct);
        if (user == null) 
            throw new DomainException("No such user");

        await _orderRepository.CreateOrder(user,
            command.Items, command.shippingAddress, ct);
        await _unitOfWork.SaveChangesAsync();
    }
}
