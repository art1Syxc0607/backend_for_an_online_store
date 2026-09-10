// Infrastructure/Services/EmailTemplateService.cs
using Application.DTOs.Email;
using Domain.Entities;
using Microsoft.Extensions.Configuration;

public class EmailTemplateService : IEmailTemplateService
{
    private readonly IConfiguration _configuration;

    public EmailTemplateService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public EmailDto CreateConfirmationEmail(User user, string token, string? returnUrl = null)
    {
        var baseUrl = _configuration["App:BaseUrl"] ?? "https://localhost:7197";
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

    public EmailDto CreateLoginNotificationEmail(User user, DateTime loginTime, string? deviceInfo = null)
    {
        return new EmailDto
        {
            To = user.Email,
            Subject = $"🔐 Вход в аккаунт {loginTime:dd.MM.yyyy HH:mm}",
            IsHtml = true,
            Body = $@"
                <html>
                    <body style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                        <div style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); 
                                    color: white; padding: 20px; text-align: center;'>
                            <h1>🔐 Новый вход в аккаунт</h1>
                        </div>
                        <div style='padding: 20px;'>
                            <h2>Здравствуйте, {user.UserName}!</h2>
                            <p>Зафиксирован вход в ваш аккаунт:</p>
                            <div style='background: #f9f9f9; padding: 15px; border-radius: 5px;'>
                                <p><strong>📅 Время:</strong> {loginTime:dd.MM.yyyy HH:mm}</p>
                                {(deviceInfo != null ? $"<p><strong>💻 Устройство:</strong> {deviceInfo}</p>" : "")}
                            </div>
                            <p style='color: #dc3545; margin-top: 20px;'>
                                ⚠️ Если это были не вы, немедленно смените пароль и свяжитесь с поддержкой.
                            </p>
                        </div>
                    </body>
                </html>"
        };
    }

    // ... остальные методы
}