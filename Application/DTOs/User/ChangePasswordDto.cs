using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.User;

public class ChangePasswordDto
{
    [Required(ErrorMessage = "Current password is required")]
    public string CurrentPassword { get; init; } = string.Empty;

    [Required(ErrorMessage = "New password is required")]
    [MinLength(6, ErrorMessage = "New password must be at least 6 characters")]
    [MaxLength(100, ErrorMessage = "New password cannot exceed 100 characters")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{6,}$",
        ErrorMessage = "Password must contain at least one uppercase, one lowercase, and one number")]
    public string NewPassword { get; init; } = string.Empty;
}