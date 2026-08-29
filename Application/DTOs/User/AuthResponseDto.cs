namespace Application.DTOs.User;

public class AuthResponseDto
{
    public string Token { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public DateTime ExpiresIn { get; set; } = DateTime.UtcNow.AddHours(1);
    public bool IsEmailConfirmed { get; set; }
}   