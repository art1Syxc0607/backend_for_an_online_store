using Application.DTOs.User;
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

public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponseDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtService _jwtService;
    private readonly ILogger<LoginCommandHandler> _logger;

    public LoginCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IJwtService jwtService, ILogger<LoginCommandHandler> logger)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
        _logger = logger;
    }

    public async Task<AuthResponseDto> Handle(LoginCommand request, CancellationToken ct)
    {
        //if (!await _userRepository.ExistsByEmailAsync(request.Email, ct))
        //    throw new DomainException("There isn't a user with this Email");

        //if (!await _userRepository.ExistsByUserNameAsync(request.UserName, ct))
        //    throw new DomainException("The user with this Name already exist");

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
            UserId = user.Id,
        };

        _logger.LogInformation(
            "Login successful: Email {Email}, UserId {UserId}, IP {IP}",
            user.Email,
            user.Id,
            request.UserIP ?? "Unknown"
        );


        return response;
    }
}

