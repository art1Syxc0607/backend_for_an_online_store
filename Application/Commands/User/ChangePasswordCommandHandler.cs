using Application.Interfaces;
using Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Commands.User;

public class ChangePasswordHandler : IRequestHandler<ChangePasswordCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<ChangePasswordHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public ChangePasswordHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        ILogger<ChangePasswordHandler> logger,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(ChangePasswordCommand request, CancellationToken ct)
    {
        _logger.LogInformation(
            "Password reset requested: UserId {UserId}, IP {IP}",
            request.UserId,
            request.UserIP ?? "Unknown"
        );

        var user = await _userRepository.GetByIdAsync(request.UserId, ct);
        if (user == null)
        {
            _logger.LogWarning(
                "Password reset failed: User not found. UserId {UserId}, IP {IP}",
                request.UserId,
                request.UserIP ?? "Unknown"
            );
            throw new DomainException("Invalid token or user");
        }

        if (!_passwordHasher.VerifyPassword(request.CurrentPassword, user.PasswordHash))
            throw new UnauthorizedAccessException("Неверный текущий пароль");

        user.UpdatePassword(_passwordHasher.HashPassword(request.NewPassword));
        await _userRepository.UpdateAsync(user, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Password reset successful: Email {Email}, UserId {UserId}, IP {IP}",
            user.Email,
            user.Id,
            request.UserIP ?? "Unknown"
        );
    }
}

