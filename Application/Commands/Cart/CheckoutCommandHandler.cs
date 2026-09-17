using Application.Commands.Email;
using Application.Commands.Order;
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

namespace Application.Commands.Cart;

public class CheckoutCommandHandler : IRequestHandler<CheckoutCommand, int> // Id of an Order
{
    private readonly ICartRepository _cartRepository;
    private readonly IUserRepository _userRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IEmailTemplateService _emailTamplate;
    private readonly IEmailBackgroundService _emailBackgroundService;
    private readonly ILogger<CheckoutCommandHandler> _logger;
    private readonly IUnitOfWork _unitOf;

    public CheckoutCommandHandler(ICartRepository cartRepository, IUserRepository userRepository, 
        IUnitOfWork unitOf, IOrderRepository orderRepository, 
        IEmailTemplateService emailTemplateService, ILogger<CheckoutCommandHandler> logger,
        IEmailBackgroundService emailBackgroundService)
    {
        _cartRepository = cartRepository;
        _userRepository = userRepository;
        _orderRepository = orderRepository;
        _emailTamplate = emailTemplateService;
        _emailBackgroundService = emailBackgroundService;
        _logger = logger;
        _unitOf = unitOf;
    }

    public async Task<int> Handle(CheckoutCommand command, CancellationToken ct)
    {
        var cart = await _cartRepository.GetByUserIdAsync(command.UserId, ct);
        if (cart == null)
            throw new DomainException("Cart not found");

        // Проверяем, что пользователь существует
        var user = await _userRepository.GetByIdAsync(command.UserId, ct);
        if (user == null)
            throw new DomainException("User not found");
        // Проверка подтверждения email
        user.EnsureEmailConfirmed();

        // Проверяем, что корзина не пуста
        if (!cart.Items.Any())
            throw new DomainException("Cannot checkout an empty cart");

        var orderItemsDto = cart.Items.Select(ci => new Domain.DTOs.Order.OrderItemDomainDto(
                ci.Product,
                ci.Quantity
            )).ToList();

        var order = new Domain.Entities.Order(user.Id, command.ShippingAddress, orderItemsDto);

        // Очищаем корзину
        //cart.Clear();

        await _orderRepository.CreateOrder(order, ct);
        await _unitOf.SaveChangesAsync();

        // ✅ Отправляем email в фоне
        try
        {
            var email = _emailTamplate.CreateCheckoutEmail(
                order,
                user,
                command.BaseUrl);

            await _emailBackgroundService.Enqueue(email);

            _logger.LogInformation(
                "Checkout confirmation email queued for {Email}, OrderId {OrderId}",
                user.Email, order.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to queue checkout email for {Email}", user.Email);
            // Не бросаем — заказ уже создан
        }

        return order.Id;
    }
}
