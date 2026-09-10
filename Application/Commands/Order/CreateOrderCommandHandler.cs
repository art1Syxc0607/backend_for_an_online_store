using Application.Commands.Email;
using Application.DTOs.Order;
using Application.Interfaces;
using Domain.DTOs.Order;
using Domain.Entities;
using Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Commands.Order;

public class CreateOrderCommandHandler : IRequestHandler
    <CreateOrderCommand, int>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUserRepository _userRepository;
    private readonly IProductRepository _productRepository;
    private readonly ILogger<CreateOrderCommandHandler> _logger;
    private readonly IEmailTemplateService _emailTamplate;
    private readonly IEmailBackgroundService _emailBackgroundService;
    private readonly IUnitOfWork _unitOfWork;

    public CreateOrderCommandHandler(
        IOrderRepository orderRepository,
        IUserRepository userRepository, IProductRepository productRepository,
        ILogger<CreateOrderCommandHandler> logger,
        IEmailTemplateService emailTamplate,
        IEmailBackgroundService emailBackgroundService,
        IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _userRepository = userRepository;
        _productRepository = productRepository;
        _logger = logger;
        _emailTamplate = emailTamplate;
        _emailBackgroundService = emailBackgroundService;
        _unitOfWork = unitOfWork;
    }

    public async Task<int> Handle(CreateOrderCommand command,
        CancellationToken ct)
    {
        _logger.LogInformation(
            "Order creation started: UserId {UserId}, ItemsCount {ItemsCount}",
            command.UserId,
            command.Items.Count
        );

        var user = await _userRepository.GetByIdAsync(command.UserId,
            ct);
        if (user == null)
        {
            _logger.LogWarning(
                "Login failed: User not found. UserId {Id}",
                command.UserId
            );
            throw new DomainException("User not found.");
        } 

        // Проверка подтверждения email
        user.EnsureEmailConfirmed();

        var productIds = command.Items.Select(i => i.ProductId).ToList();
        var products = await _productRepository.GetByIdsAsync(productIds, ct);

        if(products == null) throw new DomainException("Some products not found");
        if (products.Count != productIds.Count)
            throw new DomainException("Some products not found");

        // 2. Создаём словарь для быстрого поиска продукта по ID
        var productDict = products.ToDictionary(p => p.Id);

        // 3. Создаём OrderItemDomainDto из command.Items
        var orderItemDtos = command.Items
            .Select(item => new OrderItemDomainDto(
                productDict[item.ProductId], // ← продукт из словаря
                item.Quantity
            ))
            .ToList();


        // 4. Создаем заказ (вся бизнес-логика внутри Order)
        var order = new Domain.Entities.Order(command.UserId, command.ShippingAddress, orderItemDtos);



        await _orderRepository.CreateOrder(order, ct);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation(
            "Order created successfully: OrderId {OrderId}, UserId {UserId}, TotalAmount " +
            "{TotalAmount}, ItemsCount {ItemsCount}",
            order.Id,
            order.UserId,
            order.TotalAmount,
            order.Items.Count
        );

        var emailToSend = _emailTamplate
            .CreateOrderConfirmationEmail(order, user);

        await _emailBackgroundService.Enqueue(emailToSend);

        return order.Id;
    }
}
