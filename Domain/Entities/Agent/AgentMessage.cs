// Domain/Entities/Agent/AgentMessage.cs
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;

namespace Domain.Entities.Agent;

/// <summary>
/// Сообщение в диалоге с AI-помощником (для UI).
/// </summary>
public class AgentMessage
{
    public int Id { get; private set; }
    public int ConversationId { get; private set; }
    public AgentConversationEntity Conversation { get; private set; } = null!;

    /// <summary>
    /// Роль: "user" | "assistant" | "system" | "tool".
    /// </summary>
    public AgentMessageRole Role { get; private set; }

    /// <summary>
    /// Текст сообщения.
    /// </summary>
    public string Text { get; private set; } = string.Empty;

    public DateTime CreatedAt { get; private set; }

    private AgentMessage() { }

    public AgentMessage(int conversationId, AgentMessageRole role, string text)
    {
        ConversationId = conversationId;
        Role = role;
        Text = text;
        CreatedAt = DateTime.UtcNow;
    }

    // ✅ Фабричные методы
    public static AgentMessage User(int conversationId, string text)
        => new(conversationId, AgentMessageRole.User, text);

    public static AgentMessage Assistant(int conversationId, string text)
        => new(conversationId, AgentMessageRole.User, text);
}