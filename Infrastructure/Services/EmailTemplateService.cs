// Infrastructure/Services/EmailTemplateService.cs
using Application.DTOs.Email;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Services;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

public class EmailTemplateService : IEmailTemplateService
{
    private readonly IConfiguration _configuration;

    public EmailTemplateService(IConfiguration configuration)
    {
        _configuration = configuration;

        //// ✅ Читаем один раз при создании
        //_baseUrl = configuration["App:BaseUrl"]
        //    ?? throw new InvalidOperationException("App:BaseUrl is not configured");
    }

    private int GetPaymentTimeoutMinutes()
    {
        // ✅ Читаем из конфига с fallback на 30 минут
        return _configuration.GetValue<int>(
            "BackgroundServices:ExpiredOrders:PaymentTimeoutMinutes",
            defaultValue: 30);
    }

    public EmailDto CreateRegisterNotificationEmail(User user, string token, string baseUrl, string? returnUrl = null)
    {

        var confirmationUrl = $"{baseUrl}/api/auth/confirm-email?token={token}&userId={user.Id}";

        return new EmailDto
        {
            To = user.Email,
            Subject = "Подтверждение регистрации",
            IsHtml = true,
            Body = $@"
                <html>
                    <body style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                        <div style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); 
                                    color: white; padding: 20px; text-align: center;'>
                            <h1>📧 Подтверждение email</h1>
                        </div>
                        <div style='padding: 20px;'>
                            <h2>Здравствуйте, {user.UserName}!</h2>
                            <p>Спасибо за регистрацию в нашем магазине.</p>
                            <p>Для подтверждения email, пожалуйста, перейдите по ссылке:</p>
                            <p style='text-align: center; margin: 30px 0;'>
                                <a href='{confirmationUrl}' 
                                   style='background: #667eea; color: white; padding: 12px 30px; 
                                          text-decoration: none; border-radius: 5px;'>
                                    ✅ Подтвердить email
                                </a>
                            </p>
                            <p>Ссылка действительна в течение 24 часов.</p>
                            <p style='color: #666; font-size: 12px;'>
                                Если вы не регистрировались, проигнорируйте это письмо.
                            </p>
                        </div>
                    </body>
                </html>"
        };
    }

    public EmailDto CreateLoginNotificationEmail(
        User user,
        DateTime loginTime,
        string baseUrl,
        DeviceInfoDto? deviceInfo = null,
        LocationInfoDto? locationInfo = null)
    {
        var changePasswordUrl = $"{baseUrl}/api/auth/change-password";
        var securityUrl = $"{baseUrl}/api/auth/security";
        var supportUrl = $"{baseUrl}/api/auth/support";

        // Формируем блок с устройством
        var deviceBlock = deviceInfo != null ? $@"
        <tr>
            <td style='padding: 8px; border: 1px solid #ddd; background-color: #f9f9f9; width: 40%;'>
                <strong>💻 Устройство</strong>
            </td>
            <td style='padding: 8px; border: 1px solid #ddd;'>
                {deviceInfo.DeviceType} — {deviceInfo.DeviceModel}
            </td>
        </tr>
        <tr>
            <td style='padding: 8px; border: 1px solid #ddd; background-color: #f9f9f9;'>
                <strong>🖥️ ОС</strong>
            </td>
            <td style='padding: 8px; border: 1px solid #ddd;'>
                {deviceInfo.Os} {deviceInfo.OsVersion}
            </td>
        </tr>
        <tr>
            <td style='padding: 8px; border: 1px solid #ddd; background-color: #f9f9f9;'>
                <strong>🌐 Браузер</strong>
            </td>
            <td style='padding: 8px; border: 1px solid #ddd;'>
                {deviceInfo.Browser} {deviceInfo.BrowserVersion}
            </td>
        </tr>" : "";

        // Формируем блок с локацией
        var locationBlock = locationInfo != null &&
            (!string.IsNullOrEmpty(locationInfo.Country) ||
             !string.IsNullOrEmpty(locationInfo.City)) ? $@"
        <tr>
            <td style='padding: 8px; border: 1px solid #ddd; background-color: #f9f9f9; width: 40%;'>
                <strong>🌍 Страна</strong>
            </td>
            <td style='padding: 8px; border: 1px solid #ddd;'>
                {locationInfo.Country ?? "Не определена"}
            </td>
        </tr>
        {(string.IsNullOrEmpty(locationInfo.City) ? "" : $@"
        <tr>
            <td style='padding: 8px; border: 1px solid #ddd; background-color: #f9f9f9;'>
                <strong>🏙️ Город</strong>
            </td>
            <td style='padding: 8px; border: 1px solid #ddd;'>
                {locationInfo.City}{(string.IsNullOrEmpty(locationInfo.Region) ? "" : $", {locationInfo.Region}")}
            </td>
        </tr>")}
        {(string.IsNullOrEmpty(locationInfo.Isp) ? "" : $@"
        <tr>
            <td style='padding: 8px; border: 1px solid #ddd; background-color: #f9f9f9;'>
                <strong>📡 Провайдер</strong>
            </td>
            <td style='padding: 8px; border: 1px solid #ddd;'>
                {locationInfo.Isp}
            </td>
        </tr>")}
        {(string.IsNullOrEmpty(locationInfo.TimeZone) ? "" : $@"
        <tr>
            <td style='padding: 8px; border: 1px solid #ddd; background-color: #f9f9f9;'>
                <strong>🕐 Часовой пояс</strong>
            </td>
            <td style='padding: 8px; border: 1px solid #ddd;'>
                {locationInfo.TimeZone}
            </td>
        </tr>")}" : "";

        // Если нет ни устройства, ни локации — показываем заглушку
        var hasDetails = deviceInfo != null || locationInfo != null;
        var detailsBlock = hasDetails ? $@"
        <h3>📋 Детали входа:</h3>
        <table style='width: 100%; border-collapse: collapse; margin: 15px 0;'>
            <tbody>
                <tr>
                    <td style='padding: 8px; border: 1px solid #ddd; background-color: #f9f9f9; width: 40%;'>
                        <strong>📅 Время входа</strong>
                    </td>
                    <td style='padding: 8px; border: 1px solid #ddd;'>
                        {loginTime:dd.MM.yyyy HH:mm:ss} (UTC)
                    </td>
                </tr>
                {deviceBlock}
                {locationBlock}
            </tbody>
        </table>" : $@"
        <div class='order-details'>
            <p><strong>📅 Время входа:</strong> {loginTime:dd.MM.yyyy HH:mm:ss} (UTC)</p>
        </div>";

        var subject = $"🔐 Вход в аккаунт {loginTime:dd.MM.yyyy HH:mm}";
        var body = $@"
            <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; color: #333; line-height: 1.6; }}
                        .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 20px; text-align: center; }}
                        .content {{ padding: 20px; max-width: 600px; margin: 0 auto; }}
                        .warning-box {{ background-color: #f8d7da; padding: 15px; border-radius: 5px; margin: 20px 0; border-left: 4px solid #dc3545; }}
                        .info-box {{ background-color: #d1ecf1; padding: 15px; border-radius: 5px; margin: 15px 0; border-left: 4px solid #17a2b8; }}
                        .button {{
                            display: inline-block;
                            padding: 12px 30px;
                            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
                            color: white;
                            text-decoration: none;
                            border-radius: 5px;
                            margin: 15px 0;
                        }}
                        .button-danger {{
                            display: inline-block;
                            padding: 12px 30px;
                            background: linear-gradient(135deg, #dc3545 0%, #c82333 100%);
                            color: white;
                            text-decoration: none;
                            border-radius: 5px;
                            margin: 15px 0;
                        }}
                        .footer {{ margin-top: 30px; padding-top: 20px; border-top: 1px solid #ddd; font-size: 12px; color: #666; text-align: center; }}
                        .lock-icon {{ font-size: 48px; margin-bottom: 10px; }}
                    </style>
                </head>
                <body>
                    <div class='header'>
                        <div class='lock-icon'>🔐</div>
                        <h1>Новый вход в аккаунт</h1>
                    </div>

                    <div class='content'>
                        <h2>Здравствуйте, {user.UserName}!</h2>

                        <p>Мы зафиксировали новый вход в ваш аккаунт.</p>

                        <div class='info-box'>
                            <p style='margin: 0; color: #0c5460;'>
                                ✅ <strong>Если это были вы</strong> — просто проигнорируйте это письмо.
                            </p>
                        </div>

                        {detailsBlock}

                        <div class='warning-box'>
                            <p style='margin: 0 0 10px 0; color: #721c24;'>
                                ⚠️ <strong>Если это были не вы:</strong>
                            </p>
                            <ul style='margin: 0; padding-left: 20px; color: #721c24;'>
                                <li>Немедленно смените пароль</li>
                                <li>Включите двухфакторную аутентификацию</li>
                                <li>Свяжитесь со службой поддержки</li>
                            </ul>
                            <div style='text-align: center; margin-top: 15px;'>
                                <a href='{changePasswordUrl}' class='button-danger'>🔑 Сменить пароль</a>
                            </div>
                        </div>

                        <div style='text-align: center;'>
                            <a href='{securityUrl}' class='button'>🛡️ Настройки безопасности</a>
                        </div>

                        <p style='margin-top: 20px; color: #666; font-size: 14px;'>
                            💡 <strong>Совет:</strong> Регулярно проверяйте историю входов 
                            и активные сессии в личном кабинете.
                        </p>

                        <div style='background-color: #f8f9fa; padding: 15px; border-radius: 5px; margin: 15px 0;'>
                            <p style='margin: 0; color: #666; font-size: 13px;'>
                                📧 Если у вас есть вопросы, свяжитесь с 
                                <a href='{supportUrl}' style='color: #667eea;'>службой поддержки</a>.
                            </p>
                        </div>

                        <div class='footer'>
                            <p>Это автоматическое сообщение. Пожалуйста, не отвечайте на него.</p>
                            <p>Вы получили это письмо, потому что вход был выполнен с нового устройства.</p>
                            <p>© {DateTime.Now.Year} Интернет-магазин. Все права защищены.</p>
                        </div>
                    </div>
                </body>
            </html>
        ";

        return new EmailDto
        {
            To = user.Email,
            Subject = subject,
            Body = body,
            IsHtml = true
        };
    }

    public EmailDto CreateOrderConfirmationEmail(Order order, User user, string baseUrl)
    {
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

        var subject = $"✅ Заказ #{order.Id} успешно оформлен!";
        var body = $@"
            <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; color: #333; line-height: 1.6; }}
                        .header {{ background: linear-gradient(135deg, #4CAF50 0%, #45a049 100%); color: white; padding: 20px; text-align: center; }}
                        .content {{ padding: 20px; max-width: 600px; margin: 0 auto; }}
                        .order-details {{ background-color: #f9f9f9; padding: 15px; border-radius: 5px; margin: 15px 0; }}
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
                        .footer {{ margin-top: 30px; padding-top: 20px; border-top: 1px solid #ddd; font-size: 12px; color: #666; text-align: center; }}
                        .status-badge {{
                            display: inline-block;
                            padding: 5px 15px;
                            background-color: #ffc107;
                            color: #333;
                            border-radius: 20px;
                            font-weight: bold;
                        }}
                        .success-icon {{ font-size: 48px; margin-bottom: 10px; }}
                    </style>
                </head>
                <body>
                    <div class='header'>
                        <div class='success-icon'>✅</div>
                        <h1>Заказ успешно оформлен!</h1>
                    </div>

                    <div class='content'>
                        <h2>Здравствуйте, {user.UserName}!</h2>

                        <p>Спасибо за ваш заказ! Мы получили его и уже начали обработку.</p>

                        <div class='order-details'>
                            <p><strong>📋 Номер заказа:</strong> <strong>#{order.Id}</strong></p>
                            <p><strong>📅 Дата заказа:</strong> {order.CreatedAt:dd.MM.yyyy HH:mm}</p>
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

                        <div style='background-color: #fff3cd; padding: 15px; border-radius: 5px; margin: 15px 0; border-left: 4px solid #ffc107;'>
                            <p style='margin: 0; color: #856404; font-size: 14px;'>
                                ⏳ <strong>Что дальше?</strong> Мы свяжемся с вами для подтверждения заказа. 
                                Следите за статусом в личном кабинете.
                            </p>
                        </div>

                        <div style='text-align: center;'>
                            <a href='{orderDetailsUrl}' class='button'>📋 Посмотреть детали заказа</a>
                        </div>

                        <p style='margin-top: 20px; color: #666;'>
                            💡 Если у вас есть вопросы, свяжитесь с нашей службой поддержки.
                        </p>

                        <div class='footer'>
                            <p>Это автоматическое сообщение. Пожалуйста, не отвечайте на него.</p>
                            <p>© {DateTime.Now.Year} Интернет-магазин. Все права защищены.</p>
                        </div>
                    </div>
                </body>
            </html>
        ";

        return new EmailDto
        {
            To = user.Email,
            Subject = subject,
            Body = body,
            IsHtml = true
        };
    }

    public EmailDto CreatePasswordResetEmail(User user, string code)
    {
        return new EmailDto
        {
            To = user.Email,
            Subject = "🔐 Код для сброса пароля",
            IsHtml = true,
            Body = $@"
            <html>
                <body style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <div style='background: linear-gradient(135deg, #f093fb 0%, #f5576c 100%); 
                                color: white; padding: 20px; text-align: center;'>
                        <h1>🔐 Сброс пароля</h1>
                    </div>
                    <div style='padding: 20px;'>
                        <h2>Здравствуйте, {user.UserName}!</h2>
                        <p>Вы запросили сброс пароля. Ваш код подтверждения:</p>
                        
                        <div style='text-align: center; margin: 30px 0;'>
                            <div style='display: inline-block; background: #f5f5f5; 
                                        padding: 20px 40px; border-radius: 10px;
                                        font-size: 36px; font-weight: bold; 
                                        letter-spacing: 8px; color: #f5576c;'>
                                {code}
                            </div>
                        </div>
                        
                        <p>Код действителен в течение <strong>15 минут</strong>.</p>
                        <p style='color: #dc3545;'>
                            ⚠️ Если вы не запрашивали сброс пароля, проигнорируйте это письмо 
                            и смените пароль для безопасности.
                        </p>
                    </div>
                </body>
            </html>"
        };
    }

    public EmailDto CreateShipmentEmail(Order order, User user, string baseUrl)
    {
        
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

        return new EmailDto
        {
            To = user.Email,
            Subject = subject,
            Body = body,
            IsHtml = true
        };
    }


    public EmailDto CreateDeliveryEmail(Order order, User user, string baseUrl)
    {
        var orderDetailsUrl = $"{baseUrl}/api/orders/{order.Id}";
        var reviewUrl = $"{baseUrl}/api/reviews/create?orderId={order.Id}";

        // Формируем HTML таблицу с товарами
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
                        .header {{ background: linear-gradient(135deg, #11998e 0%, #38ef7d 100%); color: white; padding: 20px; text-align: center; }}
                        .content {{ padding: 20px; max-width: 600px; margin: 0 auto; }}
                        .order-details {{ background-color: #f9f9f9; padding: 15px; border-radius: 5px; margin: 15px 0; }}
                        .delivery-info {{ background: linear-gradient(135deg, #d4edda 0%, #c3e6cb 100%); padding: 15px; border-radius: 5px; margin: 15px 0; border-left: 4px solid #28a745; }}
                        .order-table {{ width: 100%; border-collapse: collapse; margin: 15px 0; }}
                        .order-table th {{ background: linear-gradient(135deg, #11998e 0%, #38ef7d 100%); color: white; padding: 10px; text-align: left; }}
                        .order-table td {{ padding: 8px; border: 1px solid #ddd; }}
                        .order-table tr:nth-child(even) {{ background-color: #f9f9f9; }}
                        .total-row {{ font-weight: bold; background-color: #f2f2f2; }}
                        .button {{
                            display: inline-block;
                            padding: 12px 30px;
                            background: linear-gradient(135deg, #11998e 0%, #38ef7d 100%);
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
                        .success-icon {{ font-size: 48px; margin-bottom: 10px; }}
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
                        <div class='success-icon'>🎉</div>
                        <h1>Ваш заказ доставлен!</h1>
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
                                свяжитесь с нашей службой поддержки.
                            </p>
                        </div>

                        <div style='text-align: center; margin: 20px 0;'>
                            <p style='font-size: 14px; color: #666;'>
                                🛍️ <strong>Спасибо, что выбрали нас!</strong>
                            </p>
                            <p style='font-size: 12px; color: #999;'>
                                Мы ценим каждого клиента и стараемся сделать ваш опыт покупок 
                                максимально приятным.
                            </p>
                        </div>

                        <div class='footer'>
                            <p>Спасибо за покупку! Ждем вас снова! 🛍️</p>
                            <p>Это автоматическое сообщение. Пожалуйста, не отвечайте на него.</p>
                            <p>© {DateTime.Now.Year} Интернет-магазин. Все права защищены.</p>
                        </div>
                    </div>
                </body>
            </html>
        ";

        return new EmailDto
        {
            To = user.Email,
            Subject = subject,
            Body = body,
            IsHtml = true
        };
    }

    public EmailDto CreateReceiptEmail(Order order, User user, string baseUrl)
    {
        var orderDetailsUrl = $"{baseUrl}/api/orders/{order.Id}";
        var reviewUrl = $"{baseUrl}/api/reviews/create?orderId={order.Id}";
        var promotionsUrl = $"{baseUrl}/api/promotions";
        var supportUrl = $"{baseUrl}/api/auth/support";

        // Формируем HTML таблицу с товарами
        var itemsHtml = string.Join("", order.Items.Select(item => $@"
            <tr>
                <td style='padding: 8px; border: 1px solid #ddd;'>{item.ProductNameAtPurchase}</td>
                <td style='padding: 8px; border: 1px solid #ddd; text-align: center;'>{item.Quantity}</td>
                <td style='padding: 8px; border: 1px solid #ddd; text-align: right;'>{item.PriceAtPurchase:C}</td>
                <td style='padding: 8px; border: 1px solid #ddd; text-align: right;'>{item.PriceAtPurchase * item.Quantity:C}</td>
            </tr>
        "));

        var subject = $"✅ Заказ #{order.Id} получен! Спасибо за покупку!";
        var body = $@"
            <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; color: #333; line-height: 1.6; }}
                        .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 20px; text-align: center; }}
                        .content {{ padding: 20px; max-width: 600px; margin: 0 auto; }}
                        .order-details {{ background-color: #f9f9f9; padding: 15px; border-radius: 5px; margin: 15px 0; }}
                        .receipt-info {{ background: linear-gradient(135deg, #d4edda 0%, #c3e6cb 100%); padding: 15px; border-radius: 5px; margin: 15px 0; border-left: 4px solid #28a745; }}
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
                        .success-icon {{ font-size: 48px; margin-bottom: 10px; }}
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
                        .social-links {{
                            margin: 15px 0;
                        }}
                        .social-links a {{
                            display: inline-block;
                            margin: 0 10px;
                            text-decoration: none;
                            color: #667eea;
                        }}
                    </style>
                </head>
                <body>
                    <div class='header'>
                        <div class='success-icon'>✅</div>
                        <h1>Заказ получен!</h1>
                        <p style='font-size: 18px; margin: 5px 0 0 0;'>Заказ #{order.Id}</p>
                    </div>

                    <div class='content'>
                        <h2>Здравствуйте, {user.UserName}!</h2>

                        <p style='font-size: 18px; color: #667eea;'>
                            🎉 Спасибо, что подтвердили получение заказа!
                        </p>

                        <p>Вы успешно подтвердили получение заказа <strong>#{order.Id}</strong>.</p>

                        <div class='receipt-info'>
                            <p><strong>✅ Статус заказа:</strong> <span class='status-badge'>Получен</span></p>
                            <p><strong>📅 Дата получения:</strong> {order.ReceivedAt?.ToString("dd.MM.yyyy HH:mm") ?? "Не указана"}</p>
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
                            <h3>⭐ Понравились товары?</h3>
                            <p>Оставьте отзыв о качестве товаров и сервиса!</p>
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
                                💡 <strong>Специальное предложение:</strong> 
                                <a href='{promotionsUrl}' style='color: #2e7d32;'>Получите скидку 10% на следующий заказ!</a>
                            </p>
                        </div>

                        <div style='background-color: #fff3cd; padding: 15px; border-radius: 5px; margin: 15px 0; border-left: 4px solid #ffc107;'>
                            <p style='margin: 0; color: #856404; font-size: 14px;'>
                                📋 Если у вас возникли проблемы с заказом, 
                                свяжитесь с нашей <a href='{supportUrl}' style='color: #856404;'>службой поддержки</a>.
                            </p>
                        </div>

                        <div style='text-align: center; margin: 20px 0;'>
                            <p style='font-size: 14px; color: #666;'>
                                🛍️ <strong>Спасибо, что выбрали нас!</strong>
                            </p>
                            <p style='font-size: 12px; color: #999;'>
                                Мы ценим каждого клиента и стараемся сделать ваш опыт покупок 
                                максимально приятным.
                            </p>
                        </div>

                        <div class='social-links' style='text-align: center;'>
                            <p style='font-size: 12px; color: #666;'>Следите за нами:</p>
                            <a href='{baseUrl}/social/instagram'>📸 Instagram</a>
                            <a href='{baseUrl}/social/telegram'>📱 Telegram</a>
                            <a href='{baseUrl}/social/vk'>🌐 VK</a>
                        </div>

                        <div class='footer'>
                            <p>Спасибо за покупку! Ждем вас снова! 🛍️</p>
                            <p>Это автоматическое сообщение. Пожалуйста, не отвечайте на него.</p>
                            <p>© {DateTime.Now.Year} Интернет-магазин. Все права защищены.</p>
                        </div>
                    </div>
                </body>
            </html>
        ";

        return new EmailDto
        {
            To = user.Email,
            Subject = subject,
            Body = body,
            IsHtml = true
        };
    }

    public EmailDto CreatePaymentConfirmationEmail(
    Order order,
    User user,
    Payment payment,
    string baseUrl)
    {
        var orderDetailsUrl = $"{baseUrl}/api/orders/{order.Id}";
        var receiptUrl = $"{baseUrl}/api/payments/{payment.Id}/receipt";
        var supportUrl = $"{baseUrl}/api/auth/support";

        // Формируем HTML таблицу с товарами
        var itemsHtml = string.Join("", order.Items.Select(item => $@"
            <tr>
                <td style='padding: 8px; border: 1px solid #ddd;'>{item.ProductNameAtPurchase}</td>
                <td style='padding: 8px; border: 1px solid #ddd; text-align: center;'>{item.Quantity}</td>
                <td style='padding: 8px; border: 1px solid #ddd; text-align: right;'>{item.PriceAtPurchase:C}</td>
                <td style='padding: 8px; border: 1px solid #ddd; text-align: right;'>{item.PriceAtPurchase * item.Quantity:C}</td>
            </tr>
        "));

        // Иконка метода оплаты
        var methodIcon = payment.Method switch
        {
            PaymentMethod.Card => "💳",
            PaymentMethod.GooglePay => "🅖",
            PaymentMethod.ApplePay => "",
            PaymentMethod.SBP => "🏦",
            _ => "💳"
        };

        // Название метода оплаты
        var methodName = payment.Method switch
        {
            PaymentMethod.Card => "Банковская карта",
            PaymentMethod.GooglePay => "Google Pay",
            PaymentMethod.ApplePay => "Apple Pay",
            PaymentMethod.SBP => "СБП (Система быстрых платежей)",
            _ => "Онлайн-оплата"
        };

        var subject = $"✅ Оплата заказа #{order.Id} подтверждена!";
        var body = $@"
            <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; color: #333; line-height: 1.6; }}
                        .header {{ background: linear-gradient(135deg, #11998e 0%, #38ef7d 100%); color: white; padding: 20px; text-align: center; }}
                        .content {{ padding: 20px; max-width: 600px; margin: 0 auto; }}
                        .success-box {{ background: linear-gradient(135deg, #d4edda 0%, #c3e6cb 100%); padding: 20px; border-radius: 10px; margin: 20px 0; text-align: center; border-left: 4px solid #28a745; }}
                        .payment-details {{ background-color: #f9f9f9; padding: 15px; border-radius: 5px; margin: 15px 0; }}
                        .order-table {{ width: 100%; border-collapse: collapse; margin: 15px 0; }}
                        .order-table th {{ background: linear-gradient(135deg, #11998e 0%, #38ef7d 100%); color: white; padding: 10px; text-align: left; }}
                        .order-table td {{ padding: 8px; border: 1px solid #ddd; }}
                        .order-table tr:nth-child(even) {{ background-color: #f9f9f9; }}
                        .total-row {{ font-weight: bold; background-color: #f2f2f2; }}
                        .button {{
                            display: inline-block;
                            padding: 12px 30px;
                            background: linear-gradient(135deg, #11998e 0%, #38ef7d 100%);
                            color: white;
                            text-decoration: none;
                            border-radius: 5px;
                            margin: 15px 0;
                        }}
                        .button:hover {{ opacity: 0.9; }}
                        .button-secondary {{
                            display: inline-block;
                            padding: 12px 30px;
                            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
                            color: white;
                            text-decoration: none;
                            border-radius: 5px;
                            margin: 15px 0;
                        }}
                        .footer {{ margin-top: 30px; padding-top: 20px; border-top: 1px solid #ddd; font-size: 12px; color: #666; text-align: center; }}
                        .success-icon {{ font-size: 64px; margin-bottom: 10px; }}
                        .amount-badge {{
                            display: inline-block;
                            padding: 10px 25px;
                            background: white;
                            color: #11998e;
                            border-radius: 30px;
                            font-size: 24px;
                            font-weight: bold;
                            box-shadow: 0 2px 10px rgba(0,0,0,0.1);
                        }}
                        .payment-info-table {{
                            width: 100%;
                            border-collapse: collapse;
                            margin: 15px 0;
                        }}
                        .payment-info-table td {{
                            padding: 8px 12px;
                            border-bottom: 1px solid #e0e0e0;
                        }}
                        .payment-info-table td:first-child {{
                            color: #666;
                            width: 45%;
                        }}
                        .payment-info-table td:last-child {{
                            font-weight: 500;
                            text-align: right;
                        }}
                        .transaction-id {{
                            font-family: 'Courier New', monospace;
                            background-color: #f5f5f5;
                            padding: 2px 8px;
                            border-radius: 3px;
                            font-size: 12px;
                        }}
                    </style>
                </head>
                <body>
                    <div class='header'>
                        <div class='success-icon'>✅</div>
                        <h1>Оплата подтверждена!</h1>
                    </div>

                    <div class='content'>
                        <h2>Здравствуйте, {user.UserName}!</h2>

                        <p>Ваша оплата заказа <strong>#{order.Id}</strong> успешно подтверждена.</p>

                        <div class='success-box'>
                            <p style='margin: 0 0 10px 0; color: #155724; font-size: 16px;'>
                                <strong>Сумма оплаты:</strong>
                            </p>
                            <div class='amount-badge'>
                                {payment.Amount:C}
                            </div>
                        </div>

                        <h3>💳 Детали платежа:</h3>
                        <div class='payment-details'>
                            <table class='payment-info-table'>
                                <tr>
                                    <td>💳 Метод оплаты</td>
                                    <td>{methodIcon} {methodName}</td>
                                </tr>
                                <tr>
                                    <td>📅 Дата оплаты</td>
                                    <td>{payment.PaidAt?.ToString("dd.MM.yyyy HH:mm:ss") ?? "Не указана"}</td>
                                </tr>
                                <tr>
                                    <td>💰 Сумма</td>
                                    <td><strong>{payment.Amount:C}</strong></td>
                                </tr>
                                <tr>
                                    <td>📊 Статус</td>
                                    <td><span style='color: #28a745; font-weight: bold;'>✓ Оплачено</span></td>
                                </tr>
                                <tr>
                                    <td>🆔 Транзакция</td>
                                    <td><span class='transaction-id'>{payment.TransactionId}</span></td>
                                </tr>
                                {(string.IsNullOrEmpty(payment.ExternalTransactionId) ? "" : $@"
                                <tr>
                                    <td>🔗 ID платёжной системы</td>
                                    <td><span class='transaction-id'>{payment.ExternalTransactionId}</span></td>
                                </tr>")}
                            </table>
                        </div>

                        <h3>📦 Информация о заказе:</h3>
                        <div class='payment-details'>
                            <p><strong>📋 Номер заказа:</strong> #{order.Id}</p>
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

                        <div style='background-color: #d1ecf1; padding: 15px; border-radius: 5px; margin: 15px 0; border-left: 4px solid #17a2b8;'>
                            <p style='margin: 0; color: #0c5460; font-size: 14px;'>
                                📦 <strong>Что дальше?</strong> Мы уже начали обработку вашего заказа. 
                                Как только заказ будет отправлен, вы получите уведомление.
                            </p>
                        </div>

                        <div style='text-align: center;'>
                            <a href='{orderDetailsUrl}' class='button'>📋 Посмотреть детали заказа</a>
                        </div>

                        <div style='text-align: center;'>
                            <a href='{receiptUrl}' class='button-secondary'>🧾 Скачать чек</a>
                        </div>

                        <div style='background-color: #f8f9fa; padding: 15px; border-radius: 5px; margin: 15px 0;'>
                            <p style='margin: 0; color: #666; font-size: 13px;'>
                                📧 Если у вас есть вопросы по оплате, свяжитесь с нашей 
                                <a href='{supportUrl}' style='color: #11998e;'>службой поддержки</a>, 
                                указав номер транзакции: <strong>{payment.TransactionId}</strong>.
                            </p>
                        </div>

                        <div class='footer'>
                            <p>Это автоматическое сообщение. Пожалуйста, не отвечайте на него.</p>
                            <p>Сохраните это письмо как подтверждение оплаты.</p>
                            <p>© {DateTime.Now.Year} Интернет-магазин. Все права защищены.</p>
                        </div>
                    </div>
                </body>
            </html>
        ";

        return new EmailDto
        {
            To = user.Email,
            Subject = subject,
            Body = body,
            IsHtml = true
        };
    }

    public EmailDto CreateReviewResponseNotificationEmail(
    Review review,
    User user,
    Product product,
    string baseUrl)
    {
        var reviewUrl = $"{baseUrl}/api/products/{product.Id}/reviews/{review.Id}";
        var productUrl = $"{baseUrl}/api/products/{product.Id}";
        var supportUrl = $"{baseUrl}/api/auth/support";

        // Формируем звёзды рейтинга
        var starsHtml = string.Concat(Enumerable.Range(1, 5).Select(i =>
            i <= review.Rating
                ? "<span style='color: #ffc107; font-size: 20px;'>★</span>"
                : "<span style='color: #ddd; font-size: 20px;'>★</span>"));

        // Обрезаем длинный текст отзыва
        var truncatedReviewText = review.Text.Length > 200
            ? review.Text[..200] + "..."
            : review.Text;

        var subject = $"💬 Администратор ответил на ваш отзыв о \"{product.Name}\"";
        var body = $@"
            <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; color: #333; line-height: 1.6; }}
                        .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 20px; text-align: center; }}
                        .content {{ padding: 20px; max-width: 600px; margin: 0 auto; }}
                        .review-box {{ background-color: #f9f9f9; padding: 15px; border-radius: 5px; margin: 15px 0; border-left: 4px solid #ffc107; }}
                        .response-box {{ background: linear-gradient(135deg, #e7f3ff 0%, #d1ecf1 100%); padding: 15px; border-radius: 5px; margin: 15px 0; border-left: 4px solid #17a2b8; }}
                        .product-box {{ background-color: #fff; padding: 15px; border-radius: 5px; margin: 15px 0; border: 1px solid #e0e0e0; display: flex; align-items: center; }}
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
                        .comment-icon {{ font-size: 48px; margin-bottom: 10px; }}
                        .admin-badge {{
                            display: inline-block;
                            padding: 3px 10px;
                            background-color: #17a2b8;
                            color: white;
                            border-radius: 12px;
                            font-size: 11px;
                            font-weight: bold;
                            text-transform: uppercase;
                        }}
                        .rating-stars {{
                            margin: 8px 0;
                        }}
                        .review-text {{
                            font-style: italic;
                            color: #555;
                            padding: 10px;
                            background: white;
                            border-radius: 5px;
                            margin-top: 10px;
                        }}
                        .response-text {{
                            color: #0c5460;
                            padding: 10px;
                            background: white;
                            border-radius: 5px;
                            margin-top: 10px;
                        }}
                        .response-date {{
                            font-size: 12px;
                            color: #999;
                            margin-top: 5px;
                        }}
                        .product-name {{
                            font-weight: bold;
                            color: #667eea;
                        }}
                    </style>
                </head>
                <body>
                    <div class='header'>
                        <div class='comment-icon'>💬</div>
                        <h1>Ответ на ваш отзыв</h1>
                    </div>

                    <div class='content'>
                        <h2>Здравствуйте, {user.UserName}!</h2>

                        <p>Администратор нашего магазина ответил на ваш отзыв о товаре:</p>

                        <div class='product-box'>
                            <div>
                                <p style='margin: 0;'>
                                    🛍️ <span class='product-name'>{product.Name}</span>
                                </p>
                                <p style='margin: 5px 0 0 0; font-size: 13px; color: #666;'>
                                    Артикул: {product.Sku ?? "—"}
                                </p>
                            </div>
                        </div>

                        <h3>📝 Ваш отзыв:</h3>
                        <div class='review-box'>
                            <div class='rating-stars'>
                                {starsHtml}
                                <span style='color: #666; font-size: 14px; margin-left: 5px;'>
                                    ({review.Rating}/5)
                                </span>
                            </div>
                            <div class='review-text'>
                                ""{truncatedReviewText}""
                            </div>
                            <p style='margin: 10px 0 0 0; font-size: 12px; color: #999;'>
                                📅 Опубликован: {review.CreatedAt:dd.MM.yyyy HH:mm}
                            </p>
                        </div>

                        <h3>💬 Ответ администратора:</h3>
                        <div class='response-box'>
                            <span class='admin-badge'>Администратор</span>
                            <div class='response-text'>
                                {review.AdminResponse}
                            </div>
                            <p class='response-date'>
                                📅 {review.AdminResponseAt?.ToString("dd.MM.yyyy HH:mm") ?? "Только что"}
                            </p>
                        </div>

                        <div style='background-color: #d4edda; padding: 15px; border-radius: 5px; margin: 15px 0; border-left: 4px solid #28c745;'>
                            <p style='margin: 0; color: #155724; font-size: 14px;'>
                                ✅ <strong>Спасибо за ваш отзыв!</strong> 
                                Мы ценим ваше мнение и стараемся сделать сервис лучше.
                            </p>
                        </div>

                        <div style='text-align: center;'>
                            <a href='{reviewUrl}' class='button'>💬 Посмотреть на сайте</a>
                        </div>

                        <div style='text-align: center;'>
                            <a href='{productUrl}' class='button-secondary' style='display: inline-block; padding: 12px 30px; background: linear-gradient(135deg, #11998e 0%, #38ef7d 100%); color: white; text-decoration: none; border-radius: 5px; margin: 15px 0;'>
                                🛍️ К товару
                            </a>
                        </div>

                        <div style='background-color: #f8f9fa; padding: 15px; border-radius: 5px; margin: 15px 0;'>
                            <p style='margin: 0; color: #666; font-size: 13px;'>
                                📧 Если у вас есть вопросы, свяжитесь с нашей 
                                <a href='{supportUrl}' style='color: #667eea;'>службой поддержки</a>.
                            </p>
                        </div>

                        <div class='footer'>
                            <p>Это автоматическое сообщение. Пожалуйста, не отвечайте на него.</p>
                            <p>Вы получили это письмо, потому что оставили отзыв о товаре.</p>
                            <p>© {DateTime.Now.Year} Интернет-магазин. Все права защищены.</p>
                        </div>
                    </div>
                </body>
            </html>
        ";

        return new EmailDto
        {
            To = user.Email,
            Subject = subject,
            Body = body,
            IsHtml = true
        };
    }

    public EmailDto ResendEmailConfirmation(User user, string token, string baseUrl)
    {
        var confirmationUrl = $"{baseUrl}/api/auth/confirm-email?token={token}&userId={user.Id}";
        var supportUrl = $"{baseUrl}/api/auth/support";

        var subject = "📧 Подтверждение email (повторное письмо)";
        var body = $@"
            <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; color: #333; line-height: 1.6; }}
                        .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 20px; text-align: center; }}
                        .content {{ padding: 20px; max-width: 600px; margin: 0 auto; }}
                        .info-box {{ background-color: #fff3cd; padding: 15px; border-radius: 5px; margin: 15px 0; border-left: 4px solid #ffc107; }}
                        .success-box {{ background: linear-gradient(135deg, #d4edda 0%, #c3e6cb 100%); padding: 15px; border-radius: 5px; margin: 15px 0; border-left: 4px solid #28a745; }}
                        .button {{
                            display: inline-block;
                            padding: 14px 40px;
                            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
                            color: white;
                            text-decoration: none;
                            border-radius: 5px;
                            margin: 20px 0;
                            font-size: 16px;
                            font-weight: bold;
                        }}
                        .button:hover {{ opacity: 0.9; }}
                        .footer {{ margin-top: 30px; padding-top: 20px; border-top: 1px solid #ddd; font-size: 12px; color: #666; text-align: center; }}
                        .mail-icon {{ font-size: 48px; margin-bottom: 10px; }}
                        .confirmation-link {{
                            background-color: #f5f5f5;
                            padding: 12px;
                            border-radius: 5px;
                            word-break: break-all;
                            font-family: 'Courier New', monospace;
                            font-size: 12px;
                            color: #667eea;
                            margin: 15px 0;
                        }}
                        .expiry-warning {{
                            color: #dc3545;
                            font-weight: bold;
                        }}
                        .steps {{
                            background-color: #f8f9fa;
                            padding: 15px 20px;
                            border-radius: 5px;
                            margin: 15px 0;
                        }}
                        .steps ol {{
                            margin: 10px 0 0 0;
                            padding-left: 20px;
                        }}
                        .steps li {{
                            margin: 5px 0;
                            color: #555;
                        }}
                    </style>
                </head>
                <body>
                    <div class='header'>
                        <div class='mail-icon'>📧</div>
                        <h1>Подтверждение email</h1>
                    </div>

                    <div class='content'>
                        <h2>Здравствуйте, {user.UserName}!</h2>

                        <div class='info-box'>
                            <p style='margin: 0; color: #856404;'>
                                📩 Вы запросили повторную отправку письма для подтверждения email.
                            </p>
                        </div>

                        <p>Для завершения регистрации и активации аккаунта, пожалуйста, 
                           подтвердите ваш email по ссылке ниже:</p>

                        <div style='text-align: center;'>
                            <a href='{confirmationUrl}' class='button'>✅ Подтвердить email</a>
                        </div>

                        <p style='text-align: center; color: #666; font-size: 13px;'>
                            Если кнопка не работает, скопируйте ссылку ниже:
                        </p>
                        <div class='confirmation-link'>
                            {confirmationUrl}
                        </div>

                        <div class='success-box'>
                            <p style='margin: 0; color: #155724; font-size: 14px;'>
                                ⏰ <strong>Ссылка действительна в течение 24 часов.</strong>
                            </p>
                        </div>

                        <div class='steps'>
                            <p style='margin: 0; font-weight: bold; color: #333;'>
                                📋 Что нужно сделать:
                            </p>
                            <ol>
                                <li>Нажмите кнопку <strong>Подтвердить email</strong></li>
                                    < li > Дождитесь сообщения об успешном подтверждении</ li >
    
                                    < li > Войдите в личный кабинет </ li >
    
                                </ ol >
    
                            </ div >
    

                            < div style = 'background-color: #f8d7da; padding: 15px; border-radius: 5px; margin: 15px 0; border-left: 4px solid #dc3545;' >
    
                                < p style = 'margin: 0; color: #721c24; font-size: 13px;' >
                                ⚠️ < strong > Не запрашивали это письмо ?</ strong > Просто проигнорируйте его.
                                Ваш аккаунт в безопасности.
                            </ p >
                        </ div >

                        < div style = 'background-color: #e7f3ff; padding: 15px; border-radius: 5px; margin: 15px 0; border-left: 4px solid #17a2b8;' >
                            < p style = 'margin: 0; color: #0c5460; font-size: 13px;' >
                                💡 < strong > Не получается подтвердить email ?</ strong >< br >
                                Проверьте папку Спам или свяжитесь с нашей
                                <a href= '{supportUrl}' style = 'color: #17a2b8;' > службой поддержки </ a >.
                            </ p >
                        </ div >

                        < div style = 'text-align: center; margin-top: 20px;' >
                            < p style = 'font-size: 14px; color: #666;' >
                                🛍️ После подтверждения email вам откроются все возможности магазина!
                            </ p >
                        </ div >

                                < div class='footer'>
                                    <p>Это автоматическое сообщение.Пожалуйста, не отвечайте на него.</p>
                                    <p>Вы получили это письмо, потому что запросили повторное подтверждение email.</p>
                                    <p>© { DateTime.Now.Year}
            Интернет-магазин.Все права защищены.</p>
                                </div>
                            </div>
                        </body>
                    </html>
        ";

        return new EmailDto
        {
            To = user.Email,
            Subject = subject,
            Body = body,
            IsHtml = true
        };
    }

    public EmailDto CreateOrderCancelledByTimeoutEmail(
    Order order,
    User user,
    string baseUrl)
    {
        var reorderUrl = $"{baseUrl}/api/orders/{order.Id}/reorder";
        var catalogUrl = $"{baseUrl}/api/products";
        var supportUrl = $"{baseUrl}/api/auth/support";

        // Формируем HTML таблицу с товарами
        var itemsHtml = string.Join("", order.Items.Select(item => $@"
            <tr>
                <td style='padding: 8px; border: 1px solid #ddd;'>{item.ProductNameAtPurchase}</td>
                <td style='padding: 8px; border: 1px solid #ddd; text-align: center;'>{item.Quantity}</td>
                <td style='padding: 8px; border: 1px solid #ddd; text-align: right;'>{item.PriceAtPurchase:C}</td>
                <td style='padding: 8px; border: 1px solid #ddd; text-align: right;'>{item.PriceAtPurchase * item.Quantity:C}</td>
            </tr>
        "));

        var subject = $"⏰ Заказ #{order.Id} отменён (не оплачен вовремя)";
        var body = $@"
            <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; color: #333; line-height: 1.6; }}
                        .header {{ background: linear-gradient(135deg, #ffc107 0%, #ff9800 100%); color: white; padding: 20px; text-align: center; }}
                        .content {{ padding: 20px; max-width: 600px; margin: 0 auto; }}
                        .order-details {{ background-color: #f9f9f9; padding: 15px; border-radius: 5px; margin: 15px 0; }}
                        .warning-box {{ background-color: #fff3cd; padding: 15px; border-radius: 5px; margin: 15px 0; border-left: 4px solid #ffc107; }}
                        .order-table {{ width: 100%; border-collapse: collapse; margin: 15px 0; }}
                        .order-table th {{ background: linear-gradient(135deg, #ffc107 0%, #ff9800 100%); color: white; padding: 10px; text-align: left; }}
                        .order-table td {{ padding: 8px; border: 1px solid #ddd; }}
                        .order-table tr:nth-child(even) {{ background-color: #f9f9f9; }}
                        .total-row {{ font-weight: bold; background-color: #f2f2f2; }}
                        .button {{
                            display: inline-block;
                            padding: 12px 30px;
                            background: linear-gradient(135deg, #ff9800 0%, #f57c00 100%);
                            color: white;
                            text-decoration: none;
                            border-radius: 5px;
                            margin: 15px 0;
                        }}
                        .button:hover {{ opacity: 0.9; }}
                        .button-secondary {{
                            display: inline-block;
                            padding: 12px 30px;
                            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
                            color: white;
                            text-decoration: none;
                            border-radius: 5px;
                            margin: 15px 0;
                        }}
                        .footer {{ margin-top: 30px; padding-top: 20px; border-top: 1px solid #ddd; font-size: 12px; color: #666; text-align: center; }}
                        .clock-icon {{ font-size: 48px; margin-bottom: 10px; }}
                        .status-badge {{
                            display: inline-block;
                            padding: 5px 15px;
                            background-color: #dc3545;
                            color: white;
                            border-radius: 20px;
                            font-weight: bold;
                        }}
                    </style>
                </head>
                <body>
                    <div class='header'>
                        <div class='clock-icon'>⏰</div>
                        <h1>Заказ отменён</h1>
                        <p style='font-size: 18px; margin: 5px 0 0 0;'>Заказ #{order.Id}</p>
                    </div>

                    <div class='content'>
                        <h2>Здравствуйте, {user.UserName}!</h2>

                        <p>К сожалению, ваш заказ <strong>#{order.Id}</strong> был отменён, 
                           так как не был оплачен в течение {GetPaymentTimeoutMinutes()} минут.</p>

                        <div class='warning-box'>
                            <p style='margin: 0 0 10px 0; color: #856404;'>
                                💡 <strong>Товары возвращены на склад</strong> 
                                и снова доступны для покупки.
                            </p>
                            <p style='margin: 0; color: #856404; font-size: 14px;'>
                                Если вы хотите приобрести их, оформите заказ заново.
                            </p>
                        </div>

                        <div class='order-details'>
                            <p><strong>📅 Дата создания заказа:</strong> {order.CreatedAt:dd.MM.yyyy HH:mm}</p>
                            <p><strong>📅 Дата отмены:</strong> {order.CancelledAt?.ToString("dd.MM.yyyy HH:mm") ?? "Только что"}</p>
                            <p><strong>📍 Адрес доставки:</strong> {order.ShippingAddress}</p>
                            <p><strong>📊 Статус заказа:</strong> <span class='status-badge'>Отменён</span></p>
                        </div>

                        <h3>🛍️ Состав отменённого заказа:</h3>
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
                            <a href='{reorderUrl}' class='button'>🔄 Повторить заказ</a>
                        </div>

                        <div style='text-align: center;'>
                            <a href='{catalogUrl}' class='button-secondary'>🛍️ Перейти в каталог</a>
                        </div>

                        <div style='background-color: #e7f3ff; padding: 15px; border-radius: 5px; margin: 15px 0; border-left: 4px solid #17a2b8;'>
                            <p style='margin: 0; color: #0c5460; font-size: 14px;'>
                                💡 <strong>Возникли проблемы с оплатой?</strong> 
                                Свяжитесь с нашей <a href='{supportUrl}' style='color: #17a2b8;'>службой поддержки</a>, 
                                и мы поможем разобраться.
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

        return new EmailDto
        {
            To = user.Email,
            Subject = subject,
            Body = body,
            IsHtml = true
        };
    }

    // Infrastructure/Services/EmailTemplateService.cs
    public EmailDto CreateCheckoutEmail(Order order, User user, string baseUrl)
    {
        var orderDetailsUrl = $"{baseUrl}/api/orders/{order.Id}";
        var paymentUrl = $"{baseUrl}/api/orders/{order.Id}/initiate-payment";
        var catalogUrl = $"{baseUrl}/api/products";
        var supportUrl = $"{baseUrl}/api/auth/support";

        // Формируем HTML таблицу с товарами
        var itemsHtml = string.Join("", order.Items.Select(item => $@"
            <tr>
                <td style='padding: 10px; border: 1px solid #ddd;'>{item.ProductNameAtPurchase}</td>
                <td style='padding: 10px; border: 1px solid #ddd; text-align: center;'>{item.Quantity}</td>
                <td style='padding: 10px; border: 1px solid #ddd; text-align: right;'>{item.PriceAtPurchase:C}</td>
                <td style='padding: 10px; border: 1px solid #ddd; text-align: right;'>{item.PriceAtPurchase * item.Quantity:C}</td>
            </tr>
        "));

        // Определяем, есть ли уже оплата
        var isPaid = order.Status == OrderStatus.Paid;
        var statusText = isPaid ? "Оплачен" : "Ожидает оплаты";
        var statusColor = isPaid ? "#28a745" : "#ffc107";
        var statusIcon = isPaid ? "✅" : "⏳";

        var subject = isPaid
            ? $"✅ Заказ #{order.Id} оформлен и оплачен!"
            : $"🛒 Заказ #{order.Id} оформлен! Ожидает оплаты";

        var body = $@"
            <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; color: #333; line-height: 1.6; }}
                        .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 25px; text-align: center; }}
                        .content {{ padding: 20px; max-width: 600px; margin: 0 auto; }}
                        .order-details {{ background-color: #f9f9f9; padding: 15px; border-radius: 5px; margin: 15px 0; }}
                        .status-box {{ background-color: {(isPaid ? "#d4edda" : "#fff3cd")}; padding: 15px; border-radius: 5px; margin: 15px 0; border-left: 4px solid {statusColor}; }}
                        .order-table {{ width: 100%; border-collapse: collapse; margin: 15px 0; }}
                        .order-table th {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 12px; text-align: left; }}
                        .order-table td {{ padding: 10px; border: 1px solid #ddd; }}
                        .order-table tr:nth-child(even) {{ background-color: #f9f9f9; }}
                        .total-row {{ font-weight: bold; background-color: #f2f2f2; }}
                        .button {{
                            display: inline-block;
                            padding: 14px 35px;
                            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
                            color: white;
                            text-decoration: none;
                            border-radius: 5px;
                            margin: 15px 0;
                            font-weight: bold;
                        }}
                        .button-pay {{
                            display: inline-block;
                            padding: 14px 35px;
                            background: linear-gradient(135deg, #28a745 0%, #20c997 100%);
                            color: white;
                            text-decoration: none;
                            border-radius: 5px;
                            margin: 15px 0;
                            font-weight: bold;
                        }}
                        .button:hover, .button-pay:hover {{ opacity: 0.9; }}
                        .footer {{ margin-top: 30px; padding-top: 20px; border-top: 1px solid #ddd; font-size: 12px; color: #666; text-align: center; }}
                        .cart-icon {{ font-size: 48px; margin-bottom: 10px; }}
                        .status-badge {{
                            display: inline-block;
                            padding: 6px 18px;
                            background-color: {statusColor};
                            color: {(isPaid ? "white" : "#333")};
                            border-radius: 20px;
                            font-weight: bold;
                        }}
                        .info-box {{
                            background-color: #e7f3ff;
                            padding: 15px;
                            border-radius: 5px;
                            margin: 15px 0;
                            border-left: 4px solid #17a2b8;
                        }}
                    </style>
                </head>
                <body>
                    <div class='header'>
                        <div class='cart-icon'>🛒</div>
                        <h1>Заказ оформлен!</h1>
                        <p style='font-size: 18px; margin: 5px 0 0 0;'>Заказ #{order.Id}</p>
                    </div>

                    <div class='content'>
                        <h2>Здравствуйте, {user.UserName}!</h2>

                        <p>Спасибо за оформление заказа! Мы получили его и уже начали обработку.</p>

                        <div class='status-box'>
                            <p style='margin: 0; color: #333; font-size: 16px;'>
                                {statusIcon} <strong>Статус заказа:</strong> 
                                <span class='status-badge'>{statusText}</span>
                            </p>
                            {(isPaid
                                    ? "<p style='margin: 10px 0 0 0; color: #155724;'>✅ Оплата получена! Заказ передан в обработку.</p>"
                                    : "<p style='margin: 10px 0 0 0; color: #856404;'>⏳ Не забудьте оплатить заказ. После оплаты он будет передан в обработку.</p>")}
                        </div>

                        <div class='order-details'>
                            <p><strong>📋 Номер заказа:</strong> #{order.Id}</p>
                            <p><strong>📅 Дата оформления:</strong> {order.CreatedAt:dd.MM.yyyy HH:mm}</p>
                            <p><strong>📍 Адрес доставки:</strong> {order.ShippingAddress}</p>
                            <p><strong>💰 Сумма к оплате:</strong> <strong style='color: #667eea; font-size: 18px;'>{order.TotalAmount:C}</strong></p>
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
                                    <td colspan='3' style='text-align: right; padding: 12px;'>
                                        <strong>Итого:</strong>
                                    </td>
                                    <td style='text-align: right; padding: 12px;'>
                                        <strong>{order.TotalAmount:C}</strong>
                                    </td>
                                </tr>
                            </tfoot>
                        </table>

                        {(!isPaid ? $@"
                        <div style='text-align: center;'>
                            <a href='{paymentUrl}' class='button-pay'>💳 Оплатить заказ</a>
                        </div>
                        " : "")}

                        <div style='text-align: center;'>
                            <a href='{orderDetailsUrl}' class='button'>📋 Посмотреть детали заказа</a>
                        </div>

                        <div class='info-box'>
                            <p style='margin: 0; color: #0c5460; font-size: 14px;'>
                                💡 <strong>Что дальше?</strong>
                                {(isPaid
                                        ? "Мы передадим заказ в обработку и сообщим, когда он будет отправлен."
                                        : "После оплаты заказ будет передан в обработку. Как только он будет отправлен, вы получите уведомление.")}
                            </p>
                        </div>

                        <div style='background-color: #f8f9fa; padding: 15px; border-radius: 5px; margin: 15px 0;'>
                            <p style='margin: 0; color: #666; font-size: 13px;'>
                                📧 Если у вас есть вопросы, свяжитесь с нашей 
                                <a href='{supportUrl}' style='color: #667eea;'>службой поддержки</a>, 
                                указав номер заказа: <strong>#{order.Id}</strong>.
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

        return new EmailDto
        {
            To = user.Email,
            Subject = subject,
            Body = body,
            IsHtml = true
        };
    }
}