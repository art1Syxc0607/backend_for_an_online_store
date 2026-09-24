using Application.DTOs.Email;
using Application.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly SmtpSettings _smtpSettings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(
        IOptions<SmtpSettings> smtpSettings,
        ILogger<EmailService> logger)
    {
        _smtpSettings = smtpSettings.Value;
        _logger = logger;
    }

    public async Task SendEmailAsync(
        string to,
        string subject,
        string body,
        bool isHtml = true)
    {
        await SendEmailAsync(new EmailDto
        {
            To = to,
            Subject = subject,
            Body = body,
            IsHtml = isHtml
        });
    }

    public async Task SendEmailAsync(EmailDto dto)
    {
        try
        {
            // 1. Создаём сообщение
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(
                _smtpSettings.FromName,
                _smtpSettings.FromEmail));
            message.To.Add(MailboxAddress.Parse(dto.To));
            message.Subject = dto.Subject;

            // 2. Тело письма
            var bodyBuilder = new BodyBuilder();
            if (dto.IsHtml)
                bodyBuilder.HtmlBody = dto.Body;
            else
                bodyBuilder.TextBody = dto.Body;

            message.Body = bodyBuilder.ToMessageBody();

            // 3. Подключаемся к SMTP
            using var client = new SmtpClient();

            // ✅ Определяем тип шифрования по порту
            var secureSocketOptions = _smtpSettings.Port switch
            {
                465 => SecureSocketOptions.SslOnConnect,    // SSL (устаревший)
                587 => SecureSocketOptions.StartTls,        // STARTTLS (рекомендуется)
                25 => SecureSocketOptions.StartTlsWhenAvailable,
                _ => SecureSocketOptions.Auto
            };

            _logger.LogDebug(
                "Connecting to SMTP {Host}:{Port} with {Security}",
                _smtpSettings.Host,
                _smtpSettings.Port,
                secureSocketOptions);

            await client.ConnectAsync(
                _smtpSettings.Host,
                _smtpSettings.Port,
                secureSocketOptions);

            // 4. Аутентификация
            await client.AuthenticateAsync(
                _smtpSettings.Username,
                _smtpSettings.Password);

            // 5. Отправка
            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            _logger.LogInformation("✅ Email sent to {To}", dto.To);
        }
        catch (AuthenticationException ex)
        {
            _logger.LogError(ex,
                "❌ SMTP authentication failed for {To}. " +
                "Check Username/Password (App Password for Gmail)",
                dto.To);
            throw;
        }
        catch (SmtpCommandException ex)
        {
            _logger.LogError(ex,
                "❌ SMTP command error for {To}: {StatusCode} - {Message}",
                dto.To, ex.StatusCode, ex.Message);
            throw;
        }
        catch (SmtpProtocolException ex)
        {
            _logger.LogError(ex,
                "❌ SMTP protocol error for {To}: {Message}",
                dto.To, ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to send email to {To}", dto.To);
            throw;
        }
    }
}

public class SmtpSettings
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = string.Empty;
    public bool EnableSsl { get; set; } = true;
}