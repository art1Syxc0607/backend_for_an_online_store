
namespace Application.DTOs.Agent;

public class ConversationDto
{
    public string SessionId { get; set; } = string.Empty;
    public string? Title { get; set; }
    public int MessageCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? LastMessageAt { get; set; }
}