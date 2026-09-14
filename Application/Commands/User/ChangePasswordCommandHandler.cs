using Application.Interfaces;
using Domain.Entities;
using Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Commands.User;

public class ChangePasswordHandler : IRequestHandler<ChangePasswordCommand, Unit>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordResetRepository _resetRepository;
    private readonly IPasswordResetService _resetService;
    private readonly IEmailBackgroundService _emailBackgroundService;
    private readonly IEmailTemplateService _emailTemplateService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ChangePasswordHandler> _logger;

    public ChangePasswordHandler(
        IUserRepository userRepository,
        IPasswordResetRepository resetRepository,
        IPasswordResetService resetService,
        IEmailBackgroundService emailService,
        IEmailTemplateService emailTemplateService,
        IUnitOfWork unitOfWork,
        ILogger<ChangePasswordHandler> logger)
    {
        _userRepository = userRepository;
        _resetRepository = resetRepository;
        _resetService = resetService;
        _emailBackgroundService = emailService;
        _emailTemplateService = emailTemplateService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Unit> Handle(ChangePasswordCommand command, CancellationToken ct)
    {
        var user = await _userRepository.GetByEmailAsync(command.Email, ct);

        // ✅ ВАЖНО: не раскрываем, существует ли пользователь!
        // Всегда возвращаем успех, даже если email не найден
        if (user == null)
        {
            _logger.LogWarning("Password reset requested for non-existent email: {Email}", command.Email);
            return Unit.Value;
        }

        // ✅ Rate limiting: не чаще 1 раза в 60 секунд
        var recentCode = await _resetRepository.GetLatestByUserIdAsync(user.Id, ct);
        if (recentCode != null && recentCode.CreatedAt > DateTime.UtcNow.AddSeconds(-60))
        {
            _logger.LogWarning("Rate limit: too many reset requests for user {UserId}", user.Id);
            throw new DomainException("Please wait before requesting a new code");
        }

        // Генерируем и хешируем код
        var code = _resetService.GenerateCode();
        var codeHash = _resetService.HashCode(code);

        // Сохраняем в БД
        var resetCode = new PasswordResetCode(user.Id, codeHash, command.IpAddress);
        await _resetRepository.AddAsync(resetCode, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        // Отправляем email уведомление
        try
        {
            var emailToSend = _emailTemplateService.CreatePasswordResetEmail(user, code);
            await _emailBackgroundService.Enqueue(emailToSend);
            _logger.LogInformation("Password reset code sent to {Email}", user.Email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send reset email code to {Email}", user.Email);
            // Не бросаем исключение, чтобы не откатывать транзакцию
            // Логируем ошибку, но заказ уже отправлен
        }

        return Unit.Value;
    }
}

