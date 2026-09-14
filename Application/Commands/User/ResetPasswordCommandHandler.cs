using Application.Interfaces;
using Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Commands.User;

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, Unit>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordResetRepository _resetRepository;
    private readonly IPasswordResetService _resetService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ResetPasswordCommandHandler> _logger;

    public ResetPasswordCommandHandler(IUserRepository userRepository, 
        IPasswordResetRepository resetRepository, IPasswordResetService resetService, 
        IPasswordHasher passwordHasher, IUnitOfWork unitOfWork, 
        ILogger<ResetPasswordCommandHandler> logger)
    {
        _userRepository = userRepository;
        _resetRepository = resetRepository;
        _resetService = resetService;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Unit> Handle(ResetPasswordCommand command, CancellationToken ct)
    {
        var user = await _userRepository.GetByEmailAsync(command.Email, ct);
        if (user == null)
        {
            // ✅ Не раскрываем, существует ли email
            throw new DomainException("Invalid or expired reset code");
        }

        // Получаем последний активный код
        var resetCode = await _resetRepository.GetLatestByUserIdAsync(user.Id, ct);

        if (resetCode == null || !resetCode.IsValid)
        {
            _logger.LogWarning("Invalid reset attempt for user {UserId}", user.Id);
            throw new DomainException("Invalid or expired reset code");
        }

        // ✅ Проверяем код
        if (!_resetService.VerifyCode(command.Code, resetCode.CodeHash))
        {
            resetCode.IncrementAttempts();
            await _unitOfWork.SaveChangesAsync(ct);

            _logger.LogWarning("Wrong reset code for user {UserId}, attempt {Attempt}",
                user.Id, resetCode.AttemptsCount);

            throw new DomainException("Invalid or expired reset code");
        }

        // ✅ Всё ок — меняем пароль
        var newPasswordHash = _passwordHasher.HashPassword(command.NewPassword);
        user.ChangePassword(newPasswordHash);

        resetCode.MarkAsUsed();

        // ✅ Инвалидируем все refresh tokens (если есть)
        // await _tokenRepository.RevokeAllUserTokensAsync(user.Id, ct);

        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("Password successfully reset for user {UserId}", user.Id);

        return Unit.Value;
    }
}
