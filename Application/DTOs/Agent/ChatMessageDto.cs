

namespace Application.DTOs.Agent;

public class ChatMessageDto
{
    public string Role { get; set; } = string.Empty;  // "user" | "assistant"
    public string Text { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
