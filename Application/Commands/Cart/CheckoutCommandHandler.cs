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
    private readonly IProductRepository _productRepository;
    private readonly IEmailTemplateService _emailTamplate;
    private readonly IEmailBackgroundService _emailBackgroundService;
    private readonly ILogger<CheckoutCommandHandler> _logger;
    private readonly IUnitOfWork _unitOf;

    public CheckoutCommandHandler(ICartRepository cartRepository, IUserRepository userRepository,
        IUnitOfWork unitOf, IOrderRepository orderRepository, IProductRepository productRepository,
        IEmailTemplateService emailTemplateService, ILogger<CheckoutCommandHandler> logger,
        IEmailBackgroundService emailBackgroundService)
    {
        _cartRepository = cartRepository;
        _userRepository = userRepository;
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _emailTamplate = emailTemplateService;
        _emailBackgroundService = emailBackgroundService;
        _logger = logger;
        _unitOf = unitOf;
    }

    public async Task<int> Handle(CheckoutCommand command, CancellationToken ct)
    {
        var user = await _userRepository.GetByIdAsync(command.UserId, ct);
        if (user == null)
            throw new DomainException("User not found");

        // Проверка подтверждения email
        user.EnsureEmailConfirmed();

        await _unitOf.BeginTransactionAsync(ct);

        try
        {
            // Lock the cart row. Concurrent checkout requests for the same user
            // will wait here and then see an already cleared cart.
            var cart = await _cartRepository.GetByUserIdForCheckoutAsync(command.UserId, ct);
            if (cart == null)
                throw new DomainException("Cart not found");

            if (!cart.Items.Any())
                throw new DomainException("Cannot checkout an empty cart");

            // Lock products in ascending Id order. This makes the
            // check-availability-and-reserve sequence atomic and prevents
            // deadlocks between carts containing the same products.
            var quantities = cart.Items
                .GroupBy(i => i.ProductId)
                .ToDictionary(g => g.Key, g => g.Sum(i => i.Quantity));

            var products = await _productRepository.GetByIdsForUpdateAsync(
                quantities.Keys,
                ct);

            if (products.Count != quantities.Count)
                throw new DomainException("Some products not found");

            var productById = products.ToDictionary(p => p.Id);

            var orderItemsDto = quantities
                .OrderBy(x => x.Key)
                .Select(x => new OrderItemDomainDto(
                    productById[x.Key],
                    x.Value))
                .ToList();

            var order = new Domain.Entities.Order(user.Id, command.ShippingAddress, orderItemsDto);

            // The order insert and cart cleanup must be committed together.
            cart.Clear();

            await _orderRepository.CreateOrder(order, ct);
            await _unitOf.SaveChangesAsync(ct);
            await _unitOf.CommitTransactionAsync(ct);

            // ✅ Отправляем email только после успешного COMMIT
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
        catch
        {
            await _unitOf.RollbackTransactionAsync(CancellationToken.None);
            throw;
        }
    }
}
