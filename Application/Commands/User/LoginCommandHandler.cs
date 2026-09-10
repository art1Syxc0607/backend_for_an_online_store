using Application.DTOs.Email;
using Application.DTOs.User;
using Application.Interfaces;
using Domain.Exceptions;
using Infrastructure.Services;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Commands.User;

public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponseDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtService _jwtService;
    private readonly ILogger<LoginCommandHandler> _logger;
    private readonly ICurrentRequestService _currentRequest;
    private readonly ILocationService _locationService;
    private readonly IDeviceInfoService _deviceInfoService;
    private readonly IEmailBackgroundService _emailBackgroundService; // ← Внедряем фоновый сервис!

    public LoginCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IJwtService jwtService, ILogger<LoginCommandHandler> logger,
    ICurrentRequestService currentRequestService, ILocationService location,
    IDeviceInfoService deviceInfoService, IEmailBackgroundService emailBackgroundService)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
        _logger = logger;
        _currentRequest = currentRequestService;
        _locationService = location;
        _deviceInfoService = deviceInfoService;
        _emailBackgroundService = emailBackgroundService;
    }

    public async Task<AuthResponseDto> Handle(LoginCommand request, CancellationToken ct)
    {
        // Логируем попытку входа
        _logger.LogInformation(
            "Login attempt: Email {Email}, IP {IP}",
            request.Email,
            request.UserIP ?? "Unknown"
        );

        var user = await _userRepository.GetByEmailAsync(request.Email, ct);

        // Пользователь не найден
        if (user == null)
        {
            _logger.LogWarning(
                "Login failed: User not found. Email {Email}, IP {IP}",
                request.Email,
                request.UserIP ?? "Unknown"
            );
            throw new DomainException("Invalid email or password.");
        }

        // Пароль неверный
        if (!_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            _logger.LogWarning(
                "Login failed: Invalid password. Email {Email}, IP {IP}, UserId {UserId}",
                request.Email,
                request.UserIP ?? "Unknown",
                user.Id
            );
            throw new DomainException("Invalid email or password.");
        }

        // Пользователь заблокирован
        if (!user.IsActive)
        {
            _logger.LogWarning(
                "Login failed: User is blocked. Email {Email}, IP {IP}, UserId {UserId}, BlockReason {BlockReason}",
                request.Email,
                request.UserIP ?? "Unknown",
                user.Id,
                user.BlockReason ?? "No reason"
            );
            throw new DomainException("Your account has been blocked. Please contact support.");
        }

        var response = new AuthResponseDto
        {
            Email = request.Email,
            UserName = user.UserName,
            Token = _jwtService.GenerateToken(user),
            ExpiresIn = DateTime.UtcNow.AddHours(1),
            UserId = user.Id,
        };

        _logger.LogInformation(
            "Login successful: Email {Email}, UserId {UserId}, IP {IP}",
            user.Email,
            user.Id,
            request.UserIP ?? "Unknown"
        );

        // ✅ Получаем локацию
        var location = await _locationService.GetLocationByIpAsync(request.UserIP ?? "Unknown", ct);

        // ✅ Получаем информацию об устройстве
        var userAgent = _currentRequest.GetUserAgent();
        var deviceInfo = _deviceInfoService.GetDeviceInfo(userAgent);

        var time = DateTime.UtcNow.ToString("dd.MM.yyyy HH:mm:ss UTC");
        var emailBody = $@"
                <html>
                    <body style='font-family: Arial, sans-serif;'>
                        <h2>🔐 Вход в аккаунт</h2>
                        <p>Здравствуйте, <strong>{user.UserName}</strong>!</p>
                        <p>В ваш аккаунт был выполнен вход.</p>
                        
                        <table style='border-collapse: collapse; width: 100%; max-width: 500px;'>
                            <tr>
                                <td style='padding: 8px; background: #f5f5f5; font-weight: bold;'>Время:</td>
                                <td style='padding: 8px;'>{time}</td>
                            </tr>
                            <tr>
                                <td style='padding: 8px; background: #f5f5f5; font-weight: bold;'>IP-адрес:</td>
                                <td style='padding: 8px;'>{request.UserIP ?? "Unknown"}</td>
                            </tr>
                            <tr>
                                <td style='padding: 8px; background: #f5f5f5; font-weight: bold;'>Устройство:</td>
                                <td style='padding: 8px;'>{deviceInfo.FullInfo}</td>
                            </tr>
                            <tr>
                                <td style='padding: 8px; background: #f5f5f5; font-weight: bold;'>Тип устройства:</td>
                                <td style='padding: 8px;'>{deviceInfo.DeviceType}</td>
                            </tr>
                            <tr>
                                <td style='padding: 8px; background: #f5f5f5; font-weight: bold;'>Местоположение:</td>
                                <td style='padding: 8px;'>{location}</td>
                            </tr>
                            <tr>
                                <td style='padding: 8px; background: #f5f5f5; font-weight: bold;'>Провайдер:</td>
                                <td style='padding: 8px;'>{location?.Isp ?? "Не определено"}</td>
                            </tr>
                        </table>

                        <p style='margin-top: 20px; color: #d32f2f; font-weight: bold;'>
                            ⚠️ Если это были не вы, немедленно смените пароль и свяжитесь с поддержкой.
                        </p>
                        <p style='color: #999; font-size: 12px;'>
                            Это автоматическое сообщение. Пожалуйста, не отвечайте на него.
                        </p>
                    </body>
                </html>
            ";

        // добавление в очередь фонового сервиса для отправки Email
        await _emailBackgroundService.Enqueue(new EmailDto
        {
            To = user.Email,
            Subject = $"🔐 Вход в аккаунт {time}",
            Body = emailBody,
            IsHtml = true
        });

        return response;
    }

    //private async Task SendLoginNotificationEmailAsync(
    //    Domain.Entities.User user,
    //    LoginCommand request,
    //    LocationInfo? location,
    //    DeviceInfoDto deviceInfo,
    //    CancellationToken ct)
    //{
    //    try
    //    {
    //        var time = DateTime.UtcNow.ToString("dd.MM.yyyy HH:mm:ss UTC");

    //        var locationStr = location != null
    //            ? $"{location.City}, {location.Country}"
    //            : "Не определено";

    //        var emailBody = $@"
    //            <html>
    //                <body style='font-family: Arial, sans-serif;'>
    //                    <h2>🔐 Вход в аккаунт</h2>
    //                    <p>Здравствуйте, <strong>{user.UserName}</strong>!</p>
    //                    <p>В ваш аккаунт был выполнен вход.</p>
                        
    //                    <table style='border-collapse: collapse; width: 100%; max-width: 500px;'>
    //                        <tr>
    //                            <td style='padding: 8px; background: #f5f5f5; font-weight: bold;'>Время:</td>
    //                            <td style='padding: 8px;'>{time}</td>
    //                        </tr>
    //                        <tr>
    //                            <td style='padding: 8px; background: #f5f5f5; font-weight: bold;'>IP-адрес:</td>
    //                            <td style='padding: 8px;'>{request.UserIP ?? "Unknown"}</td>
    //                        </tr>
    //                        <tr>
    //                            <td style='padding: 8px; background: #f5f5f5; font-weight: bold;'>Устройство:</td>
    //                            <td style='padding: 8px;'>{deviceInfo.FullInfo}</td>
    //                        </tr>
    //                        <tr>
    //                            <td style='padding: 8px; background: #f5f5f5; font-weight: bold;'>Тип устройства:</td>
    //                            <td style='padding: 8px;'>{deviceInfo.DeviceType}</td>
    //                        </tr>
    //                        <tr>
    //                            <td style='padding: 8px; background: #f5f5f5; font-weight: bold;'>Местоположение:</td>
    //                            <td style='padding: 8px;'>{locationStr}</td>
    //                        </tr>
    //                        <tr>
    //                            <td style='padding: 8px; background: #f5f5f5; font-weight: bold;'>Провайдер:</td>
    //                            <td style='padding: 8px;'>{location?.Isp ?? "Не определено"}</td>
    //                        </tr>
    //                    </table>

    //                    <p style='margin-top: 20px; color: #d32f2f; font-weight: bold;'>
    //                        ⚠️ Если это были не вы, немедленно смените пароль и свяжитесь с поддержкой.
    //                    </p>
    //                    <p style='color: #999; font-size: 12px;'>
    //                        Это автоматическое сообщение. Пожалуйста, не отвечайте на него.
    //                    </p>
    //                </body>
    //            </html>
    //        ";

    //        await _emailService.SendEmailAsync(new EmailDto
    //        {
    //            To = user.Email,
    //            Subject = $"🔐 Вход в аккаунт {time}",
    //            Body = emailBody,
    //            IsHtml = true
    //        });
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogWarning(ex, "Failed to send login notification email to {Email}", user.Email);
    //    }
    //}
}

