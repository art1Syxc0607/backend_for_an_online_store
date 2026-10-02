

using Domain.Enums;

namespace Application.DTOs.Agent;

public class ChatMessageDto
{
    public AgentMessageRole Role { get; set; }  // "user" | "assistant"
    public string Text { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
