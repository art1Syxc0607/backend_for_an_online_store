using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.User;

public class RegisterDto
{
    [Required]
    public string UserName { get; set; }
    [Required]
    [EmailAddress]
    public string Email { get; set; }
    [Required]
    [MinLength(8)]
    [MaxLength(50)]
    public string Password { get; set; } 
}

