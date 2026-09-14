
using Application.DTOs.User;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Application.Commands.User;

public class ChangePasswordCommand : IRequest<Unit>
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    
    public string? IpAddress { get; set; }
}

