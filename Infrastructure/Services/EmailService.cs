using System.Net;
using System.Net.Mail;
using Application.DTOs.Email;
using Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly SmtpSettings _smtpSettings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _smtpSettings = configuration.GetSection("Smtp").Get<SmtpSettings>()!;
        _logger = logger;
    }

    public async Task SendEmailAsync(string to, string subject, string body, bool isHtml = true)
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
            using var client = new SmtpClient(_smtpSettings.Host, _smtpSettings.Port)
            {
                EnableSsl = _smtpSettings.EnableSsl,
                Credentials = new NetworkCredential(_smtpSettings.Username, _smtpSettings.Password),
                Timeout = 10000 // 10 секунд
            };

            using var message = new MailMessage
            {
                From = new MailAddress(_smtpSettings.FromEmail, _smtpSettings.FromName),
                Subject = dto.Subject,
                Body = dto.Body,
                IsBodyHtml = dto.IsHtml
            };

            message.To.Add(dto.To);

            await client.SendMailAsync(message);
            _logger.LogInformation("✅ Email sent to {To}", dto.To);
        }
        catch (SmtpException smtpEx)
        {
            _logger.LogError(smtpEx, "SMTP error sending email to {To}: {StatusCode}", dto.To, smtpEx.StatusCode);
            throw new Exception($"SMTP error: {smtpEx.Message}", smtpEx);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {To}", dto.To);
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