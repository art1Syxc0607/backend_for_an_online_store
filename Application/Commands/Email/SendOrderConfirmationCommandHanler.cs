using Application.DTOs.Email;
using Application.Interfaces;
using Domain.Exceptions;
using MediatR;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Commands.Email;

// Application/Commands/Email/SendOrderConfirmationHandler.cs
public class SendOrderConfirmationHandler : IRequestHandler<SendOrderConfirmationCommand>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUserRepository _userRepository;
    private readonly IEmailService _emailService;
    private readonly IUnitOfWork _unitOfWork;

    public SendOrderConfirmationHandler(
        IOrderRepository orderRepository,
        IUserRepository userRepository,
        IEmailService emailService,
        IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _userRepository = userRepository;
        _emailService = emailService;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(SendOrderConfirmationCommand command, CancellationToken ct)
    {
        // 1. Получаем заказ
        var order = await _orderRepository.GetOrder(command.OrderId, ct);
        if (order == null)
            throw new DomainException("Order not found");

        // 2. Получаем пользователя
        var user = await _userRepository.GetByIdAsync(order.UserId, ct);
        if (user == null)
            throw new DomainException("User not found");

        // 3. Генерируем письмо
        var emailDto = new EmailDto
        {
            To = user.Email,
            Subject = $"Ваш заказ #{order.Id} подтвержден!",
            Body = GenerateOrderConfirmationHtml(order, user),
            IsHtml = true
        };

        // 4. Отправляем
        await _emailService.SendEmailAsync(emailDto.To, emailDto.Subject, emailDto.Body, emailDto.IsHtml);
    }

    private string GenerateOrderConfirmationHtml(Domain.Entities.Order order, Domain.Entities.User user)
    {
        var itemsHtml = string.Join("", order.Items.Select(i =>
            $"<tr><td>{i.ProductNameAtPurchase}</td><td>{i.Quantity}</td><td>{i.PriceAtPurchase:C}</td></tr>"
        ));

        return $@"
        <html>
            <body>
                <h2>Здравствуйте, {user.UserName}!</h2>
                <p>Ваш заказ <strong>#{order.Id}</strong> успешно оформлен.</p>
                
                <h3>Детали заказа:</h3>
                <p><strong>Дата:</strong> {order.CreatedAt:dd.MM.yyyy HH:mm}</p>
                <p><strong>Адрес доставки:</strong> {order.ShippingAddress}</p>
                
                <h3>Товары:</h3>
                <table border='1' cellpadding='5'>
                    <tr><th>Товар</th><th>Кол-во</th><th>Цена</th></tr>
                    {itemsHtml}
                    <tr><td colspan='2'><strong>Итого:</strong></td><td><strong>{order.TotalAmount:C}</strong></td></tr>
                </table>
                
                <p>Статус заказа: <strong>{order.Status}</strong></p>
                <p>Спасибо за покупку!</p>
            </body>
        </html>";
    }
}