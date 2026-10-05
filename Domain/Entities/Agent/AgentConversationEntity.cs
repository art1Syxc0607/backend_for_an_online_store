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
    /// <summary>
    /// Уникальный ID сессии (GUID), используется в MAF.
    /// </summary>
    public string AgentConversationId { get; private set; } = string.Empty;

    // ✅ Опциональный UserId (null для гостей)
    public int? UserId { get; private set; }
    public User? User { get; private set; }

    // ✅ GuestId для гостей
    public string? GuestId { get; private set; }

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

    // ✅ Оставляем LastMessageAt — по нему считаем "неактивность"
    public DateTime? LastMessageAt { get; private set; }


    public int MessageCount { get; private set; }

    // navigation properties
    private readonly List<AgentMessage> _messages = new();
    public IReadOnlyCollection<AgentMessage> Messages
        => _messages.AsReadOnly();

    private AgentConversationEntity() { }

    // ✅ Для авторизованного
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

    // ✅ Конструктор для гостя — без ExpiresAt
    public AgentConversationEntity(string guestId, string sessionId)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
            throw new DomainException("SessionId cannot be empty");

        if (string.IsNullOrWhiteSpace(guestId))
            throw new DomainException("GuestId cannot be empty");

        GuestId = guestId;
        AgentConversationId = sessionId;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        MessageCount = 0;
        LastMessageAt = null;  // ← будет установлено при первом сообщении
    }

    // ✅ Владельцы
    public bool IsOwnedBy(int userId) => UserId == userId;
    public bool IsOwnedByGuest(string guestId) => GuestId == guestId;
    // ✅ Хелперы для проверки
    public bool IsGuest => UserId == null;

    /// <summary>
    /// Проверяет, истёк ли диалог (неактивен N дней).
    /// </summary>
    public bool IsExpired(int retentionDays)
    {
        if (!IsGuest)
            return false;  // ✅ Авторизованные не истекают

        var lastActivity = LastMessageAt ?? CreatedAt;
        return lastActivity < DateTime.UtcNow.AddDays(-retentionDays);
    }

    /// <summary>
    /// Возвращает дату, когда диалог будет удалён.
    /// </summary>
    public DateTime? GetExpiresAt(int retentionDays)
    {
        if (!IsGuest)
            return null;

        var lastActivity = LastMessageAt ?? CreatedAt;
        return lastActivity.AddDays(retentionDays);
    }

    // ✅ Привязка к пользователю
    public void AttachToUser(int userId)
    {
        if (UserId.HasValue)
            throw new DomainException("Conversation already belongs to a user");

        UserId = userId;
        GuestId = null;
        UpdatedAt = DateTime.UtcNow;
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

    public void IncrementMessageCount(int count = 1)
    {
        MessageCount += count;
        UpdatedAt = DateTime.UtcNow;
    }
}
