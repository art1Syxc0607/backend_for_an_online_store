
using Application.DTOs.User;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Application.Commands.User;

public class ChangePasswordCommand : IRequest
{
    [Required]
    public int UserId { get; set; }
    [Required]
    [MinLength(8)]
    [MaxLength(50)]
    public string CurrentPassword { get; set; } = string.Empty;
    [Required]
    [MinLength(8)]
    [MaxLength(50)]
    public string NewPassword { get; set; } = string.Empty;

    [Required]
    public string UserIP { get; init; } = "Unknown";
}

