using Application.Interfaces;
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

public class DeliverOrderCommandHandler : IRequestHandler<DeliverOrderCommand>
{

    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserRepository _userRepository;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<DeliverOrderCommandHandler> _logger;


    public DeliverOrderCommandHandler(IOrderRepository orderRepository, 
        IUserRepository userRepository, IEmailService emailService, IConfiguration configuration,
        ILogger<DeliverOrderCommandHandler> logger,
        IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _userRepository = userRepository;
        _emailService = emailService;
        _configuration = configuration;
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeliverOrderCommand command, CancellationToken ct)
    {
        _logger.LogInformation("Delivery started for OrderId {OrderId}", command.OrderId);

        // 1. Получаем заказ
        var order = await _orderRepository.GetOrder(command.OrderId, ct);
        if (order == null)
        {
            _logger.LogWarning("Order not found: OrderId {OrderId}", command.OrderId);
            throw new DomainException($"Order with ID {command.OrderId} not found");
        }

        // 2. Получаем пользователя
        var user = await _userRepository.GetByIdAsync(order.UserId, ct);
        if (user == null)
        {
            _logger.LogWarning("User not found: UserId {UserId}", order.UserId);
            throw new DomainException($"User with ID {order.UserId} not found");
        }

        // 3. Обновляем статус заказа на "Доставлен"
        order.Deliver();

        // 4. Сохраняем изменения
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation(
            "Order delivered successfully: OrderId {OrderId}, UserId {UserId}",
            order.Id,
            order.UserId
        );

        // 5. Отправляем email уведомление о доставке
        try
        {
            await SendDeliveryConfirmationEmailAsync(order, user);
            _logger.LogInformation("Delivery confirmation email sent to {Email}", user.Email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send delivery confirmation email for OrderId {OrderId}", order.Id);
            // Не бросаем исключение, чтобы не откатывать транзакцию
            // Заказ уже отмечен как доставленный
        }
    }
    private async Task SendDeliveryConfirmationEmailAsync(Domain.Entities.Order order, Domain.Entities.User user)
    {
        var baseUrl = _configuration["App:BaseUrl"] ?? "https://localhost:7197";
        var orderDetailsUrl = $"{baseUrl}/api/orders/{order.Id}";
        var reviewUrl = $"{baseUrl}/api/reviews/create?orderId={order.Id}";

        var itemsHtml = string.Join("", order.Items.Select(item => $@"
            <tr>
                <td style='padding: 8px; border: 1px solid #ddd;'>{item.ProductNameAtPurchase}</td>
                <td style='padding: 8px; border: 1px solid #ddd; text-align: center;'>{item.Quantity}</td>
                <td style='padding: 8px; border: 1px solid #ddd; text-align: right;'>{item.PriceAtPurchase:C}</td>
                <td style='padding: 8px; border: 1px solid #ddd; text-align: right;'>{item.PriceAtPurchase * item.Quantity:C}</td>
            </tr>
        "));

        var subject = $"🎉 Заказ #{order.Id} доставлен!";
        var body = $@"
            <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; color: #333; line-height: 1.6; }}
                        .header {{ background: linear-gradient(135deg, #4CAF50 0%, #45a049 100%); color: white; padding: 20px; text-align: center; }}
                        .content {{ padding: 20px; max-width: 600px; margin: 0 auto; }}
                        .order-details {{ background-color: #f9f9f9; padding: 15px; border-radius: 5px; margin: 15px 0; }}
                        .delivery-info {{ background-color: #d4edda; padding: 15px; border-radius: 5px; margin: 15px 0; border-left: 4px solid #28a745; }}
                        .order-table {{ width: 100%; border-collapse: collapse; margin: 15px 0; }}
                        .order-table th {{ background: linear-gradient(135deg, #4CAF50 0%, #45a049 100%); color: white; padding: 10px; text-align: left; }}
                        .order-table td {{ padding: 8px; border: 1px solid #ddd; }}
                        .order-table tr:nth-child(even) {{ background-color: #f9f9f9; }}
                        .total-row {{ font-weight: bold; background-color: #f2f2f2; }}
                        .button {{
                            display: inline-block;
                            padding: 12px 30px;
                            background: linear-gradient(135deg, #4CAF50 0%, #45a049 100%);
                            color: white;
                            text-decoration: none;
                            border-radius: 5px;
                            margin: 15px 0;
                        }}
                        .button:hover {{ opacity: 0.9; }}
                        .button-secondary {{
                            display: inline-block;
                            padding: 12px 30px;
                            background: linear-gradient(135deg, #ff6b6b 0%, #ee5a24 100%);
                            color: white;
                            text-decoration: none;
                            border-radius: 5px;
                            margin: 15px 0;
                        }}
                        .footer {{ margin-top: 30px; padding-top: 20px; border-top: 1px solid #ddd; font-size: 12px; color: #666; text-align: center; }}
                        .status-badge {{
                            display: inline-block;
                            padding: 5px 15px;
                            background-color: #28a745;
                            color: white;
                            border-radius: 20px;
                            font-weight: bold;
                        }}
                        .star {{
                            color: #ffc107;
                            font-size: 24px;
                        }}
                        .review-section {{
                            background-color: #f8f9fa;
                            padding: 20px;
                            border-radius: 10px;
                            margin: 20px 0;
                            text-align: center;
                        }}
                    </style>
                </head>
                <body>
                    <div class='header'>
                        <h1>🎉 Ваш заказ доставлен!</h1>
                        <p style='font-size: 18px;'>Заказ #{order.Id}</p>
                    </div>
                    
                    <div class='content'>
                        <h2>Здравствуйте, {user.UserName}!</h2>
                        
                        <p>Рады сообщить, что ваш заказ <strong>#{order.Id}</strong> успешно доставлен!</p>
                        
                        <div class='delivery-info'>
                            <p><strong>✅ Статус доставки:</strong> <span class='status-badge'>Доставлен</span></p>
                            <p><strong>📅 Дата доставки:</strong> {order.DeliveredAt?.ToString("dd.MM.yyyy HH:mm") ?? "Не указана"}</p>
                            <p><strong>📍 Адрес доставки:</strong> {order.ShippingAddress}</p>
                        </div>
                        
                        <div class='order-details'>
                            <p><strong>📅 Дата заказа:</strong> {order.CreatedAt:dd.MM.yyyy HH:mm}</p>
                            <p><strong>📊 Статус заказа:</strong> {order.Status}</p>
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
                        
                        <div class='review-section'>
                            <h3>⭐ Понравился заказ?</h3>
                            <p>Оставьте отзыв о товарах и помогите другим покупателям!</p>
                            <div style='font-size: 30px; margin: 10px 0;'>
                                <span class='star'>★</span>
                                <span class='star'>★</span>
                                <span class='star'>★</span>
                                <span class='star'>★</span>
                                <span class='star'>★</span>
                            </div>
                            <a href='{reviewUrl}' class='button-secondary'>✍️ Написать отзыв</a>
                        </div>
                        
                        <div style='text-align: center;'>
                            <a href='{orderDetailsUrl}' class='button'>📋 Детали заказа</a>
                        </div>
                        
                        <div style='background-color: #e8f5e9; padding: 15px; border-radius: 5px; margin: 15px 0; border-left: 4px solid #4CAF50;'>
                            <p style='margin: 0; color: #2e7d32; font-size: 14px;'>
                                💡 <strong>Совет:</strong> Если у вас есть вопросы по заказу, 
                                свяжитесь с нашей <a href='{baseUrl}/api/auth/support' style='color: #2e7d32;'>службой поддержки</a>.
                            </p>
                        </div>
                        
                        <div class='footer'>
                            <p>Спасибо, что выбрали нас! Ждем вас снова! 🛍️</p>
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
