using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.User;

public class ChangePasswordDto
{
    //[Required(ErrorMessage = "Current password is required")]
    //public string CurrentPassword { get; init; } = string.Empty;

    [Required(ErrorMessage = "Email is required")]
    public string Email { get; init; } = string.Empty;

}