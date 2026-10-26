using Application.Commands.Email;
using Application.Interfaces;
using Domain.DTOs.Order;
using Domain.Entities;
using Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Commands.Order;

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, int>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUserRepository _userRepository;
    private readonly IProductRepository _productRepository;
    private readonly IEmailTemplateService _emailTamplate;
    private readonly IEmailBackgroundService _emailBackgroundService;
    private readonly ILogger<CreateOrderCommandHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public CreateOrderCommandHandler(IOrderRepository orderRepository, IUserRepository userRepository,
        IProductRepository productRepository, IEmailTemplateService emailTamplate,
        IEmailBackgroundService emailBackgroundService, ILogger<CreateOrderCommandHandler> logger,
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

    public async Task<int> Handle(CreateOrderCommand command, CancellationToken ct)
    {
        _logger.LogInformation(
            "Order creation started: UserId {UserId}, ItemsCount {ItemsCount}",
            command.UserId,
            command.Items.Count
        );

        var user = await _userRepository.GetByIdAsync(command.UserId, ct);
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

        await _unitOfWork.BeginTransactionAsync(ct);

        try
        {
            var productIds = command.Items
                .Select(i => i.ProductId)
                .Distinct()
                .OrderBy(id => id)
                .ToList();

            // Lock and refresh products before the order constructor checks
            // AvailableQuantity and reserves stock.
            var products = await _productRepository.GetByIdsForUpdateAsync(productIds, ct);

            if (products.Count != productIds.Count)
                throw new DomainException("Some products not found");

            var productDict = products.ToDictionary(p => p.Id);

            var orderItemDtos = command.Items
                .Select(item => new OrderItemDomainDto(
                    productDict[item.ProductId],
                    item.Quantity
                ))
                .ToList();

            var order = new Domain.Entities.Order(command.UserId, command.ShippingAddress, orderItemDtos);

            await _orderRepository.CreateOrder(order, ct);
            await _unitOfWork.SaveChangesAsync(ct);
            await _unitOfWork.CommitTransactionAsync(ct);

            _logger.LogInformation(
                "Order created successfully: OrderId {OrderId}, UserId {UserId}, TotalAmount " +
                "{TotalAmount}, ItemsCount {ItemsCount}",
                order.Id,
                order.UserId,
                order.TotalAmount,
                order.Items.Count
            );

            // Отправляем email только после успешного COMMIT
            try
            {
                var emailToSend = _emailTamplate
                    .CreateOrderConfirmationEmail(order, user, command.BaseUrl);
                await _emailBackgroundService.Enqueue(emailToSend);

                _logger.LogInformation("Order confirmation sent to {Email}", user.Email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send Order confirmation to {Email}", user.Email);
                // Не бросаем исключение — заказ уже создан
            }

            return order.Id;
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(CancellationToken.None);
            throw;
        }
    }
}
