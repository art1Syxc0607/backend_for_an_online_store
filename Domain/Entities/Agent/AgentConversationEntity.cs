using Domain.Enums;
using Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Agent;

public class AgentConversationEntity
{
    public int Id { get; private set; }
    public int UserId { get; private set; }
    public User User { get; private set; } = null!;

    /// <summary>
    /// Уникальный ID сессии (GUID), используется в MAF.
    /// </summary>
    public string AgentConversationId { get; private set; } = string.Empty;

    /// <summary>
    /// Заголовок диалога (генерируется из первого сообщения).
    /// </summary>
    public string? Title { get; private set; }

    /// <summary>
    /// Сериализованное состояние сессии MAF (JSON).
    /// </summary>
    public string JsonState { get; private set; } = string.Empty;

    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public DateTime? LastMessageAt { get; private set; }

    public int MessageCount { get; private set; }

    // navigation properties
    private readonly List<AgentMessage> _messages = new();
    public IReadOnlyCollection<AgentMessage> Messages
        => _messages.AsReadOnly();

    private AgentConversationEntity() { }

    public AgentConversationEntity(int userId, string sessionId)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
            throw new DomainException("SessionId cannot be empty");

        UserId = userId;
        AgentConversationId = sessionId;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        MessageCount = 0;
    }

    // ✅ Метод добавления
    public void AddMessage(AgentMessageRole role, string text)
    {
        _messages.Add(new AgentMessage(Id, role, text));
        MessageCount++;
        LastMessageAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateState(string jsonState)
    {
        if (string.IsNullOrWhiteSpace(jsonState))
            throw new DomainException("JsonState cannot be empty");

        JsonState = jsonState;
        MessageCount++;
        LastMessageAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            return;

        // Обрезаем до 100 символов
        Title = title.Length > 100 ? title[..100] + "..." : title;
        UpdatedAt = DateTime.UtcNow;
    }
}
