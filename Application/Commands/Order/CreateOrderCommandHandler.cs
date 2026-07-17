using MediatR;
using Application.Interfaces;
using Domain.Exceptions;
using Domain.Entities;

namespace Application.Commands.Order;

public class CreateOrderCommandHandler : IRequestHandler
    <CreateOrderCommand, int>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUserRepository _userRepository;
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateOrderCommandHandler(
        IOrderRepository orderRepository,
        IUnitOfWork unitOfWork, IUserRepository userRepository, IProductRepository productRepository)
    {
        _orderRepository = orderRepository;
        _userRepository = userRepository;
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<int> Handle(CreateOrderCommand command,
        CancellationToken ct)
    {
        var user = await _userRepository.GetByIdAsync(command.UserId,
            ct);
        if (user == null) 
            throw new DomainException("No such user");

        foreach(var item in command.Items)
        {
            var product = await _productRepository.GetByIdAsync(item.ProductId, ct);


        }



        var orderitems = command.Items.Select(ot_dto => new OrderItem())
        var order = new Domain.Entities.Order(user, command.shippingAddress);


        await _orderRepository.CreateOrder(order, ct);

        await _unitOfWork.SaveChangesAsync();

        return order.Id;
    }
}
