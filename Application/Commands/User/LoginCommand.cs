using Application.DTOs.User;
using MediatR;

namespace Application.Commands.User;

public class LoginCommand : IRequest<AuthResponseDto>
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;

}

