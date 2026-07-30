using Application.Commands.Email;
using Application.Interfaces;
using Domain.DTOs.Order;
using Domain.Entities;
using Domain.Exceptions;
using MediatR;

namespace Application.Commands.Order;

public class CreateOrderCommandHandler : IRequestHandler
    <CreateOrderCommand, int>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUserRepository _userRepository;
    private readonly IProductRepository _productRepository;
    private readonly IEmailService _emailService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMediator _mediator;

    public CreateOrderCommandHandler(
        IOrderRepository orderRepository,
        IUnitOfWork unitOfWork, IUserRepository userRepository, IProductRepository productRepository,
        IEmailService emailService, IMediator mediator)
    {
        _orderRepository = orderRepository;
        _userRepository = userRepository;
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
        _emailService = emailService;
        _mediator = mediator;
    }

    public async Task<int> Handle(CreateOrderCommand command,
        CancellationToken ct)
    {
        var user = await _userRepository.GetByIdAsync(command.UserId,
            ct);
        if (user == null) 
            throw new DomainException("No such user");

        // Проверка подтверждения email
        user.EnsureEmailConfirmed();

        var productIds = command.Items.Select(i => i.ProductId).ToList();
        var products = await _productRepository.GetByIdsAsync(productIds, ct);

        if(products == null) throw new DomainException("Some products not found");
        if (products.Count != productIds.Count)
            throw new DomainException("Some products not found");


        // 3. Создаем DTO для заказа (с продуктами)
        var orderItems = command.Items
            .Select(i => new Domain.DTOs.Order.OrderItemDto
                (
                    products.First(p => p.Id == i.ProductId),   // ← передаем продукт
                    i.Quantity,
                    i.PriceAtPurchase
                )
            )
            .ToList();


        // 4. Создаем заказ (вся бизнес-логика внутри Order)
        var order = new Domain.Entities.Order(user, command.ShippingAddress, orderItems);



        await _orderRepository.CreateOrder(order, ct);
        await _unitOfWork.SaveChangesAsync();
        
        var createdOrderEmailCommand = new SendOrderConfirmationCommand { OrderId = order.Id };
        await _mediator.Send(command, ct);

        return order.Id;
    }
}
