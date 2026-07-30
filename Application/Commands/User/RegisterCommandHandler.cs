using Application.DTOs.Email;
using Application.DTOs.User;
using Application.Interfaces;
using Domain.Entities;
using Microsoft.Extensions.Configuration;
using Domain.Exceptions;
using MediatR;

namespace Application.Commands.User;

public class RegisterHandler : IRequestHandler<RegisterCommand, AuthResponseDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtService _jwtService;
    private readonly IEmailService _emailService;
    private readonly ITokenGenerator _tokenGenerator;
    private readonly IConfiguration _configuration;
    private readonly IUnitOfWork _unitOfWork;

    public RegisterHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtService jwtService,
        IEmailService emailService,
        ITokenGenerator tokenGenerator,
        IConfiguration configuration,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
        _emailService = emailService;
        _tokenGenerator = tokenGenerator;
        _configuration = configuration;
        _unitOfWork = unitOfWork;
    }

    public async Task<AuthResponseDto> Handle(RegisterCommand command, CancellationToken ct)
    {
        // 1. Проверка email
        if (await _userRepository.ExistsByEmailAsync(command.Email, ct))
            throw new DomainException("Email already registered");

        // 2. Проверка username
        if (await _userRepository.ExistsByUserNameAsync(command.UserName, ct))
            throw new DomainException("Username already taken");

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

        // 6. Отправляем письмо с подтверждением
        await SendConfirmationEmailAsync(user, token, null);

        // 7. Возвращаем токен (чтобы пользователь мог войти сразу)
        var jwtToken = _jwtService.GenerateToken(user);

        return new AuthResponseDto
        {
            Token = jwtToken,
            UserId = user.Id,
            Email = user.Email,
            UserName = user.UserName,
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

