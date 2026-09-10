using Application.DTOs.Order;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
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

public class InitiatePaymentHandler : IRequestHandler<InitiatePaymentCommand, PaymentResult>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUserRepository _userRepository;
    private readonly IPaymentService _paymentService;
    private readonly IPaymentRepository _paymentRepository;
    private readonly ILogger<InitiatePaymentHandler> _logger;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;
    private readonly IUnitOfWork _unitOfWork;

    public InitiatePaymentHandler(
        IOrderRepository orderRepository,
        IUserRepository userRepository,
        IPaymentService paymentService,
        IPaymentRepository paymentRepository,
        ILogger<InitiatePaymentHandler> logger,
        IEmailService emailService,
        IConfiguration configuration,
        IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _userRepository = userRepository;
        _paymentRepository = paymentRepository;
        _paymentService = paymentService;
        _logger = logger;
        _emailService = emailService;
        _configuration = configuration;
        _unitOfWork = unitOfWork;
    }

    public async Task<PaymentResult> Handle(InitiatePaymentCommand command, CancellationToken ct)
    {
        _logger.LogInformation(
            "Payment initiation started: OrderId {OrderId}, UserId {UserId}, " +
            "Method {Method}",
            command.OrderId,
            command.UserId,
            command.Method
        );

        // 1. Проверяем заказ
        var order = await _orderRepository.GetOrder(command.OrderId, ct);
        if (order == null)
            throw new DomainException("Order not found");

        if (order.UserId != command.UserId)
            throw new UnauthorizedAccessException("This order doesn't belong to you");

        if (order.Status == OrderStatus.Paid)
            throw new DomainException("Order is already paid");
        if (order.Status == OrderStatus.Cancelled)
            throw new DomainException("Cannot pay for a cancelled order");

        // 2. Получаем пользователя для отправки email
        var user = await _userRepository.GetByIdAsync(command.UserId, ct);
        if (user == null)
            throw new DomainException("User not found");

        // 3. Инициируем оплату через внешний сервис
        var result = await _paymentService.InitiatePaymentAsync(command.OrderId, order.TotalAmount, command.Method, ct);

        if (result.Success)
        {
            _logger.LogInformation(
                "Payment initiated successfully: OrderId {OrderId}, UserId {UserId}, " +
                "Method {Method}, PaymentIntentId {PaymentIntentId}",
                command.OrderId,
                command.UserId,
                command.Method,
                result.PaymentIntentId
            );
        }
        else
        {
            _logger.LogWarning(
                "Payment initiation failed: OrderId {OrderId}, UserId {UserId}, " +
                "Method {Method}, Error {Error}",
                command.OrderId,
                command.UserId,
                command.Method,
                result.ErrorMessage
            );
        }

        // 4. Создаем Payment со статусом Pending
        var payment = new Payment(
            order.Id,
            order.TotalAmount,
            command.Method,
            result.PaymentIntentId
        );


        await _paymentRepository.AddAsync(payment, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        _logger.LogInformation(
            "Payment saved successfully: OrderId {OrderId}, UserId {UserId}, " +
            "Method {Method}, PaymentIntentId {PaymentIntentId}",
            command.OrderId,
            command.UserId,
            command.Method,
            result.PaymentIntentId
        );

        if (result.Success)
        {
            // 5. Отправляем email уведомление об инициации оплаты (только при успехе)
            try
            {
                await SendPaymentInitiationEmailAsync(order, user, command.Method.ToString(), result.PaymentIntentId);
                _logger.LogInformation("Payment initiation email sent to {Email}", user.Email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send payment initiation email for OrderId {OrderId}", order.Id);
                // Не бросаем исключение, чтобы не прерывать процесс оплаты
            }
        }


        return result;
    }

    private async Task SendPaymentInitiationEmailAsync(Domain.Entities.Order order, Domain.Entities.User user, string paymentMethod, string paymentIntentId)
    {
        var baseUrl = _configuration["App:BaseUrl"] ?? "https://localhost:7197";
        var orderDetailsUrl = $"{baseUrl}/api/orders/{order.Id}";
        var paymentUrl = $"{baseUrl}/api/payments/{paymentIntentId}/complete";

        var itemsHtml = string.Join("", order.Items.Select(item => $@"
            <tr>
                <td style='padding: 8px; border: 1px solid #ddd;'>{item.ProductNameAtPurchase}</td>
                <td style='padding: 8px; border: 1px solid #ddd; text-align: center;'>{item.Quantity}</td>
                <td style='padding: 8px; border: 1px solid #ddd; text-align: right;'>{item.PriceAtPurchase:C}</td>
                <td style='padding: 8px; border: 1px solid #ddd; text-align: right;'>{item.PriceAtPurchase * item.Quantity:C}</td>
            </tr>
        "));

        var methodIcon = paymentMethod.ToLower() switch
        {
            "card" => "💳",
            "cash" => "💰",
            "bank_transfer" => "🏦",
            "crypto" => "₿",
            _ => "💳"
        };

        var subject = $"💳 Оплата заказа #{order.Id} - ожидает подтверждения";
        var body = $@"
            <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; color: #333; line-height: 1.6; }}
                        .header {{ background: linear-gradient(135deg, #f093fb 0%, #f5576c 100%); color: white; padding: 20px; text-align: center; }}
                        .content {{ padding: 20px; max-width: 600px; margin: 0 auto; }}
                        .order-details {{ background-color: #f9f9f9; padding: 15px; border-radius: 5px; margin: 15px 0; }}
                        .payment-info {{ background-color: #fff3cd; padding: 15px; border-radius: 5px; margin: 15px 0; border-left: 4px solid #ffc107; }}
                        .order-table {{ width: 100%; border-collapse: collapse; margin: 15px 0; }}
                        .order-table th {{ background: linear-gradient(135deg, #f093fb 0%, #f5576c 100%); color: white; padding: 10px; text-align: left; }}
                        .order-table td {{ padding: 8px; border: 1px solid #ddd; }}
                        .order-table tr:nth-child(even) {{ background-color: #f9f9f9; }}
                        .total-row {{ font-weight: bold; background-color: #f2f2f2; }}
                        .button {{
                            display: inline-block;
                            padding: 12px 30px;
                            background: linear-gradient(135deg, #f093fb 0%, #f5576c 100%);
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
                            background-color: #ffc107;
                            color: #333;
                            border-radius: 20px;
                            font-weight: bold;
                        }}
                    </style>
                </head>
                <body>
                    <div class='header'>
                        <h1>💳 Инициация оплаты</h1>
                    </div>
                    
                    <div class='content'>
                        <h2>Здравствуйте, {user.UserName}!</h2>
                        
                        <p>Вы инициировали оплату заказа <strong>#{order.Id}</strong>.</p>
                        
                        <div class='payment-info'>
                            <p><strong>💳 Метод оплаты:</strong> {methodIcon} {paymentMethod}</p>
                            <p><strong>💰 Сумма к оплате:</strong> <strong style='font-size: 18px; color: #f5576c;'>{order.TotalAmount:C}</strong></p>
                            <p><strong>📊 Статус платежа:</strong> <span class='status-badge'>Ожидает подтверждения</span></p>
                            <p><strong>🆔 ID платежа:</strong> {paymentIntentId}</p>
                        </div>
                        
                        <div class='order-details'>
                            <p><strong>📅 Дата заказа:</strong> {order.CreatedAt:dd.MM.yyyy HH:mm}</p>
                            <p><strong>📍 Адрес доставки:</strong> {order.ShippingAddress}</p>
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
                        
                        <div style='text-align: center;'>
                            <a href='{paymentUrl}' class='button'>✅ Завершить оплату</a>
                        </div>
                        
                        <p style='margin-top: 20px; color: #666; font-size: 14px;'>
                            ⏳ <strong>Важно:</strong> Платеж ожидает подтверждения. 
                            Вы можете завершить оплату по ссылке выше.
                        </p>
                        
                        <div style='background-color: #f8d7da; padding: 15px; border-radius: 5px; margin: 15px 0; border-left: 4px solid #dc3545;'>
                            <p style='margin: 0; color: #721c24; font-size: 14px;'>
                                ⚠️ Если вы не инициировали эту оплату, пожалуйста, 
                                <a href='{baseUrl}/api/auth/support' style='color: #dc3545;'>свяжитесь с поддержкой</a>.
                            </p>
                        </div>
                        
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