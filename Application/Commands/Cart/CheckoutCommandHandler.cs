using Application.Interfaces;
using MediatR;
using Domain.Exceptions;
using Domain.Entities;
using Domain.DTOs.Order;
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
    private readonly IUnitOfWork _unitOf;

    public CheckoutCommandHandler(ICartRepository cartRepository, IUserRepository userRepository, 
        IUnitOfWork unitOf, IOrderRepository orderRepository)
    {
        _cartRepository = cartRepository;
        _userRepository = userRepository;
        _orderRepository = orderRepository;
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

        var orderItemsDto = cart.Items.Select(ci => new Domain.DTOs.Order.OrderItemDto(
                ci.Product,
                ci.Quantity,
                ci.Product.Price
            )).ToList();

        var order = new Domain.Entities.Order(user, command.ShippingAddress, orderItemsDto);

        // Очищаем корзину
        cart.Clear();

        await _orderRepository.CreateOrder(order, ct);
        await _unitOf.SaveChangesAsync();

        return order.Id;
    }
}
