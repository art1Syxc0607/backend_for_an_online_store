using Application.DTOs.Email;
using Application.DTOs.User;
using Application.Interfaces;
using Domain.Entities;
using Microsoft.Extensions.Configuration;
using Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Commands.User;

public class RegisterHandler : IRequestHandler<RegisterCommand, AuthResponseDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtService _jwtService;
    private readonly IEmailService _emailService;
    private readonly ITokenGenerator _tokenGenerator;
    private readonly IConfiguration _configuration;
    private readonly ILogger<RegisterHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public RegisterHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtService jwtService,
        IEmailService emailService,
        ITokenGenerator tokenGenerator,
        IConfiguration configuration,
        ILogger<RegisterHandler> logger,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
        _emailService = emailService;
        _tokenGenerator = tokenGenerator;
        _configuration = configuration;
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task<AuthResponseDto> Handle(RegisterCommand command, CancellationToken ct)
    {
        _logger.LogInformation(
            "Register attempt: Email {Email}, Selected UserName {UserName}, UserIP {IP}",
            command.Email,
            command.UserName,
            command.UserIP
        );

        // 1. Проверка email
        if (await _userRepository.ExistsByEmailAsync(command.Email, ct))
        {
            _logger.LogWarning(
                "Register failed: Email already exists. Email {Email}, IP {IP}",
                command.Email,
                command.UserIP
            );
            throw new DomainException("Email already registered");
        }

        // 2. Проверка username
        if (await _userRepository.ExistsByUserNameAsync(command.UserName, ct))
        {
            _logger.LogWarning(
                "Register failed: Username already taken. UserName {UserName}, IP {IP}",
                command.UserName,
                command.UserIP
            );
            throw new DomainException("Username already taken");
        }



        // 3. Создаем пользователя
        var user = new Domain.Entities.User(
            command.Email,
            _passwordHasher.HashPassword(command.Password),
            command.UserName
        );

        // 4. Генерируем токен подтверждения
        var token = _tokenGenerator.GenerateEmailConfirmationToken();
        var expiry = DateTime.UtcNow.AddHours(24);
        user.GenerateEmailConfirmationToken(token, expiry);

        // 5. Сохраняем пользователя
        await _userRepository.AddAsync(user, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation(
            "User registered successfully: UserId {UserId}, Email {Email}, UserName {UserName}, IP {IP}",
            user.Id,
            user.Email,
            user.UserName,
            command.UserIP
        );

        // 6. Отправляем письмо с подтверждением
        try
        {
            await SendConfirmationEmailAsync(user, token, null);
            _logger.LogInformation(
                "Confirmation email sent: Email {Email}, UserId {UserId}",
                user.Email,
                user.Id
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to send confirmation email: Email {Email}, UserId {UserId}",
                user.Email,
                user.Id
            );
            // Продолжаем — письмо не критично для регистрации
        }

        // 7. Генерируем JWT токен
        var jwtToken = _jwtService.GenerateToken(user);

        _logger.LogInformation(
            "JWT generated for user: UserId {UserId}, Email {Email}",
            user.Id,
            user.Email
        );

        return new AuthResponseDto
        {
            Token = jwtToken,
            UserId = user.Id,
            Email = user.Email,
            UserName = user.UserName,
            ExpiresIn = DateTime.UtcNow.AddHours(1),
            IsEmailConfirmed = user.IsEmailConfirmed
        };
    }

    private async Task SendConfirmationEmailAsync(Domain.Entities.User user, string token, string? returnUrl)
    {
        var baseUrl = _configuration["App:BaseUrl"];
        var confirmationUrl = $"{baseUrl}/api/auth/confirm-email?token={token}&userId={user.Id}";

        //var emailDto = new EmailDto
        //{

        //};

        string To = user.Email;
        string Subject = "Подтверждение регистрации";
        string Body = $@"
                <html>
                    <body>
                        <h2>Здравствуйте, {user.UserName}!</h2>
                        <p>Спасибо за регистрацию в нашем магазине.</p>
                        <p>Для подтверждения email, пожалуйста, перейдите по ссылке:</p>
                        <p><a href='{confirmationUrl}'>Подтвердить email</a></p>
                        <p>Ссылка действительна в течение 24 часов.</p>
                        <p>Если вы не регистрировались, проигнорируйте это письмо.</p>
                    </body>
                </html>
            ";
        bool IsHtml = true;

        await _emailService.SendEmailAsync(To, Subject, Body, IsHtml);


    }
}

