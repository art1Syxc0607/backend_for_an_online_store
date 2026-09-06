using Application.Interfaces;
using Domain.Entities;
using Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
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
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<InitiateShipmentCommandHandler> _logger;

    public InitiateShipmentCommandHandler(IOrderRepository orderRepository, IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IEmailService emailService, IConfiguration configuration,
        ILogger<InitiateShipmentCommandHandler> logger)
    {
        _orderRepository = orderRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _emailService = emailService;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task Handle(InitiateShipmentCommand command, CancellationToken ct)
    {
        var order = await _orderRepository.GetOrder(command.OrderId, ct);

        if (order == null) throw new Exception("No such order");
        if (order == null)
        {
            _logger.LogWarning("Order not found: OrderId {OrderId}", command.OrderId);
            throw new DomainException($"Order with ID {command.OrderId} not found");
        }

        var user = await _userRepository.GetByIdAsync(order.UserId, ct);
        if (user == null)
        {
            _logger.LogWarning("User not found: UserId {UserId}", order.UserId);
            throw new DomainException($"User with ID {order.UserId} not found");
        }

        order.Ship();

        await _unitOfWork.SaveChangesAsync();
        _logger.LogInformation(
            "Order shipped successfully: OrderId {OrderId}, UserId {UserId}",
            order.Id,
            order.UserId
        );


        // Отправляем email уведомление
        try
        {
            await SendShipmentConfirmationEmailAsync(order, user);
            _logger.LogInformation("Shipment email sent to {Email}", user.Email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send shipment email for OrderId {OrderId}", order.Id);
            // Не бросаем исключение, чтобы не откатывать транзакцию
            // Логируем ошибку, но заказ уже отправлен
        }
    }

    private async Task SendShipmentConfirmationEmailAsync(Domain.Entities.Order order, Domain.Entities.User user)
    {
        var baseUrl = _configuration["App:BaseUrl"] ?? "https://localhost:7197";
        var orderDetailsUrl = $"{baseUrl}/api/orders/{order.Id}";

        // Формируем HTML таблицу с товарами
        var itemsHtml = string.Join("", order.Items.Select(item => $@"
            <tr>
                <td style='padding: 8px; border: 1px solid #ddd;'>{item.ProductNameAtPurchase}</td>
                <td style='padding: 8px; border: 1px solid #ddd; text-align: center;'>{item.Quantity}</td>
                <td style='padding: 8px; border: 1px solid #ddd; text-align: right;'>{item.PriceAtPurchase:C}</td>
                <td style='padding: 8px; border: 1px solid #ddd; text-align: right;'>{item.PriceAtPurchase * item.Quantity:C}</td>
            </tr>
        "));

        var subject = $"📦 Ваш заказ #{order.Id} отправлен!";
        var body = $@"
            <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; color: #333; line-height: 1.6; }}
                        .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 20px; text-align: center; }}
                        .content {{ padding: 20px; max-width: 600px; margin: 0 auto; }}
                        .order-details {{ background-color: #f9f9f9; padding: 15px; border-radius: 5px; margin: 15px 0; }}
                        .order-table {{ width: 100%; border-collapse: collapse; margin: 15px 0; }}
                        .order-table th {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 10px; text-align: left; }}
                        .order-table td {{ padding: 8px; border: 1px solid #ddd; }}
                        .order-table tr:nth-child(even) {{ background-color: #f9f9f9; }}
                        .total-row {{ font-weight: bold; background-color: #f2f2f2; }}
                        .button {{
                            display: inline-block;
                            padding: 12px 30px;
                            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
                            color: white;
                            text-decoration: none;
                            border-radius: 5px;
                            margin: 15px 0;
                        }}
                        .button:hover {{ opacity: 0.9; }}
                        .footer {{ margin-top: 30px; padding-top: 20px; border-top: 1px solid #ddd; font-size: 12px; color: #666; text-align: center; }}
                        .status-badge {{
                            display: inline-block;
                            padding: 5px 15px;
                            background-color: #2196F3;
                            color: white;
                            border-radius: 20px;
                            font-weight: bold;
                        }}
                        .info-icon {{ margin-right: 5px; }}
                    </style>
                </head>
                <body>
                    <div class='header'>
                        <h1>📦 Ваш заказ отправлен!</h1>
                    </div>
                    
                    <div class='content'>
                        <h2>Здравствуйте, {user.UserName}!</h2>
                        
                        <p>Рады сообщить, что ваш заказ <strong>#{order.Id}</strong> был отправлен.</p>
                        
                        <div class='order-details'>
                            <p><strong>📅 Дата заказа:</strong> {order.CreatedAt:dd.MM.yyyy HH:mm}</p>
                            <p><strong>📅 Дата отправки:</strong> {order.ShippedAt?.ToString("dd.MM.yyyy HH:mm") ?? "Не указана"}</p>
                            <p><strong>📍 Адрес доставки:</strong> {order.ShippingAddress}</p>
                            <p><strong>📊 Статус заказа:</strong> <span class='status-badge'>{order.Status}</span></p>
                        </div>
                        
                        <h3>🛍️ Состав заказа:</h3>
                        <table class='order-table'>
                            <thead>
                                <tr>
                                    <th>Товар</th>
                                    <th style='text-align: center;'>Кол-во</th>
                                    <th style='text-align: right;'>Цена</th>
                                    <th style='text-align: right;'>Сумма</th>
                                </tr>
                            </thead>
                            <tbody>
                                {itemsHtml}
                            </tbody>
                            <tfoot>
                                <tr class='total-row'>
                                    <td colspan='3' style='text-align: right; padding: 10px;'>
                                        <strong>Итого:</strong>
                                    </td>
                                    <td style='text-align: right; padding: 10px;'>
                                        <strong>{order.TotalAmount:C}</strong>
                                    </td>
                                </tr>
                            </tfoot>
                        </table>
                        
                        <div style='text-align: center;'>
                            <a href='{orderDetailsUrl}' class='button'>📋 Посмотреть детали заказа</a>
                        </div>
                        
                        <p style='margin-top: 20px; color: #666;'>
                            💡 Вы можете отслеживать статус заказа в вашем личном кабинете.
                        </p>
                        
                        <div class='footer'>
                            <p>Это автоматическое сообщение. Пожалуйста, не отвечайте на него.</p>
                            <p>© {DateTime.Now.Year} Интернет-магазин. Все права защищены.</p>
                        </div>
                    </div>
                </body>
            </html>
        ";

        await _emailService.SendEmailAsync(user.Email, subject, body, true);
    }
}
