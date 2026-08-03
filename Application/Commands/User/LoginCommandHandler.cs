using Application.DTOs.User;
using Application.Interfaces;
using Domain.Exceptions;
using MediatR;
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

    public LoginCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IJwtService jwtService)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
    }

    public async Task<AuthResponseDto> Handle(LoginCommand request, CancellationToken ct)
    {
        //if (!await _userRepository.ExistsByEmailAsync(request.Email, ct))
        //    throw new DomainException("There isn't a user with this Email");

        //if (!await _userRepository.ExistsByUserNameAsync(request.UserName, ct))
        //    throw new DomainException("The user with this Name already exist");

        var user = await _userRepository.GetByEmailAsync(request.Email, ct);

        if (user == null)
            throw new DomainException("Invalid email or password.");

        if (!_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
            throw new DomainException("Invalid email or password.");

        var response = new AuthResponseDto
        {
            Email = request.Email,
            UserName = user.UserName,
            Token = _jwtService.GenerateToken(user),
            UserId = user.Id,
        };

        return response;
    }
}

