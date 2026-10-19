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
        SetText(text);
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Создаёт сообщение с привязкой через навигацию.
    /// Позволяет сохранить диалог и сообщения ОДНИМ SaveChanges
    /// (EF сам проставит ConversationId после вставки диалога).
    /// </summary>
    public AgentMessage(AgentConversationEntity conversation, AgentMessageRole role, string text)
    {
        Conversation = conversation ?? throw new DomainException("Conversation cannot be null.");
        ConversationId = conversation.Id;
        Role = role;
        SetText(text);
        CreatedAt = DateTime.UtcNow;
    }

    private void SetText(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new DomainException("Text cannot be empty.");

        Text = text;
    }

    // ✅ Фабричные методы
    public static AgentMessage User(int conversationId, string text)
        => new(conversationId, AgentMessageRole.User, text);

    public static AgentMessage Assistant(int conversationId, string text)
        => new(conversationId, AgentMessageRole.Assistant, text);

    public static AgentMessage User(AgentConversationEntity conversation, string text)
        => new(conversation, AgentMessageRole.User, text);

    public static AgentMessage Assistant(AgentConversationEntity conversation, string text)
        => new(conversation, AgentMessageRole.Assistant, text);
}