using Application.DTOs.Email;
using Application.Interfaces;
using Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.Configuration;
using Domain.Entities;
using Microsoft.Extensions.Logging;


namespace Application.Commands.Email;

public class ResendConfirmationHandler : IRequestHandler<ResendConfirmationCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly IEmailService _emailService;
    private readonly ITokenGenerator _tokenGenerator;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ResendConfirmationHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public ResendConfirmationHandler(
        IUserRepository userRepository,
        IEmailService emailService,
        ITokenGenerator tokenGenerator,
        IConfiguration configuration,
        ILogger<ResendConfirmationHandler> logger,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _emailService = emailService;
        _tokenGenerator = tokenGenerator;
        _configuration = configuration;
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(ResendConfirmationCommand command, CancellationToken ct)
    {
        var user = await _userRepository.GetByEmailAsync(command.Email, ct);
        if (user == null)
            throw new DomainException("User not found");

        if (user.IsEmailConfirmed)
            throw new DomainException("Email already confirmed");

        // Проверка на спам (не чаще 1 раза в 5 минут)
        if (!user.CanResendConfirmationEmail())
            throw new DomainException("Please wait 5 minutes before requesting again");

        // Генерируем новый токен
        var token = _tokenGenerator.GenerateEmailConfirmationToken();
        var expiry = DateTime.UtcNow.AddHours(24);
        user.GenerateEmailConfirmationToken(token, expiry);

        await _unitOfWork.SaveChangesAsync(ct);

        // ✅ Отправляем письмо
        await SendConfirmationEmailAsync(user, token);

        _logger.LogInformation(
            "Confirmation email resent to {Email}, UserId {UserId}",
            user.Email,
            user.Id
        );
    }

    private async Task SendConfirmationEmailAsync(Domain.Entities.User user, string token)
    {
        var baseUrl = _configuration["App:BaseUrl"];
        var confirmationUrl = $"{baseUrl}/api/auth/confirm-email?token={token}&userId={user.Id}";

        var emailBody = $@"
            <html>
                <body>
                    <h2>Здравствуйте, {user.UserName}!</h2>
                    <p>Вы запросили повторную отправку письма для подтверждения email.</p>
                    <p>Перейдите по ссылке для подтверждения:</p>
                    <p><a href='{confirmationUrl}'>Подтвердить email</a></p>
                    <p>Ссылка действительна в течение 24 часов.</p>
                    <p>Если вы не запрашивали повторную отправку, проигнорируйте это письмо.</p>
                </body>
            </html>
        ";

        await _emailService.SendEmailAsync(
            user.Email,
            "Подтверждение email (повторная отправка)",
            emailBody,
            true
        );
    }
}
