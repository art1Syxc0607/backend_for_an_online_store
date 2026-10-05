using Application.DTOs.Agent;
using Application.Interfaces;
using Application.Interfaces.Agent;
using Domain.Entities.Agent;
using Domain.Exceptions;
using Infrastructure.Options;
using Microsoft.Agents.AI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Infrastructure.Services.Agent;

public class AgentService
{
    private readonly AIAgent _agent;
    private readonly IAgentConversationRepository _conversationRepository;
    private readonly IAgentMessageRepository _messageRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly AgentOptions _options;
    private readonly ILogger<AgentService> _logger;
    private static readonly JsonSerializerOptions JsonOptions = JsonSerializerOptions.Web;

    public AgentService(
        AIAgent agent,
        IAgentConversationRepository conversationRepository,
        IAgentMessageRepository messageRepository,
        IUnitOfWork unitOfWork,
        IOptions<AgentOptions> options,
        ILogger<AgentService> logger)
    {
        _agent = agent;
        _conversationRepository = conversationRepository;
        _messageRepository = messageRepository;
        _unitOfWork = unitOfWork;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<ChatResponse> ChatAsync(
        ChatRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
            throw new DomainException("Message is required");

        // ✅ Проверка: user или guest
        if (request.UserId == null && string.IsNullOrWhiteSpace(request.GuestId))
            throw new DomainException("Either UserId or GuestId is required");

        // 1. Находим или создаём
        AgentConversationEntity conversation;

        if (string.IsNullOrWhiteSpace(request.SessionId))
        {
            conversation = request.UserId.HasValue
                ? new AgentConversationEntity(
                    request.UserId.Value,
                    Guid.NewGuid().ToString())
                : new AgentConversationEntity(
                    request.GuestId!,
                    Guid.NewGuid().ToString());  // ← без retentionDays

            conversation.SetTitle(request.Message);
            await _conversationRepository.AddAsync(conversation, ct);
            await _unitOfWork.SaveChangesAsync(ct);
        }
        else
        {
            conversation = await _conversationRepository
                .GetByConversationIdAsync(request.SessionId, ct)
                ?? throw new DomainException(
                    $"Session {request.SessionId} not found");

            // Проверка владельца
            var isOwner = request.UserId.HasValue
                ? conversation.IsOwnedBy(request.UserId.Value)
                : conversation.IsOwnedByGuest(request.GuestId!);

            if (!isOwner)
                throw new UnauthorizedAccessException(
                    "You don't own this conversation");

            // ✅ Проверка "неактивности" вместо ExpiresAt
            if (conversation.IsExpired(_options.GuestConversationRetentionDays))
            {
                throw new DomainException(
                    $"This conversation has expired " +
                    $"(no activity for {_options.GuestConversationRetentionDays} days). " +
                    "Please start a new one.");
            }
        }

        // 2. Сохраняем сообщение пользователя
        var userMessage = AgentMessage.User(conversation.Id, request.Message);
        await _messageRepository.AddAsync(userMessage, ct);

        // 3. Восстанавливаем AgentSession
        AgentSession agentSession;

        if (string.IsNullOrWhiteSpace(conversation.JsonState))
        {
            agentSession = await _agent.CreateSessionAsync(ct);
        }
        else
        {
            var jsonElement = JsonSerializer.Deserialize<JsonElement>(
                conversation.JsonState, JsonOptions);

            agentSession = await _agent.DeserializeSessionAsync(
                jsonElement,
                jsonSerializerOptions: JsonOptions,
                cancellationToken: ct);
        }

        // 4. Запускаем агента
        var response = await _agent.RunAsync(request.Message, agentSession);

        // 5. Сохраняем ответ ассистента
        var assistantMessage = AgentMessage.Assistant(
            conversation.Id, response.Text);
        await _messageRepository.AddAsync(assistantMessage, ct);

        // 6. Обновляем состояние (внутри UpdateState обновится LastMessageAt)
        var serializedSession = await _agent.SerializeSessionAsync(agentSession, JsonOptions, ct);
        conversation.UpdateState(serializedSession.GetRawText());
        conversation.IncrementMessageCount(2);

        await _unitOfWork.SaveChangesAsync(ct);

        return new ChatResponse
        {
            Reply = response.Text,
            SessionId = conversation.AgentConversationId,
            MessageCount = conversation.MessageCount,
            IsGuest = conversation.IsGuest,
            ExpiresAt = conversation.GetExpiresAt(_options.GuestConversationRetentionDays),
            DaysUntilExpiry = conversation.IsGuest
                ? (int?)Math.Max(0,
                    (_options.GuestConversationRetentionDays
                     - (DateTime.UtcNow - (conversation.LastMessageAt ?? conversation.CreatedAt)).TotalDays))
                : null
        };
    }

    public async Task<List<ConversationDto>> GetConversationsAsync(
        int userId, CancellationToken ct)
    {
        var conversations = await _conversationRepository
            .GetUserConversationsAsync(userId, ct);

        return conversations.Select(c => new ConversationDto
        {
            SessionId = c.AgentConversationId,
            Title = c.Title,
            MessageCount = c.MessageCount,
            CreatedAt = c.CreatedAt,
            UpdatedAt = c.UpdatedAt,
            LastMessageAt = c.LastMessageAt
        }).ToList();
    }

    public async Task<List<ConversationDto>> GetGuestConversationsAsync(
        string guestId, CancellationToken ct)
    {
        var conversations = await _conversationRepository
            .GetGuestConversationsAsync(guestId, ct);

        return conversations.Select(c => new ConversationDto
        {
            SessionId = c.AgentConversationId,
            Title = c.Title,
            MessageCount = c.MessageCount,
            CreatedAt = c.CreatedAt,
            UpdatedAt = c.UpdatedAt,
            LastMessageAt = c.LastMessageAt
        }).ToList();
    }

    public async Task<ConversationDetailDto> GetConversationAsync(
        int? userId, string? guestId, string sessionId,
        CancellationToken ct = default)
    {
        var conversation = await _conversationRepository
            .GetByConversationIdAsync(sessionId, ct)
            ?? throw new DomainException($"Session {sessionId} not found");

        // ✅ Проверка владельца
        var isOwner = userId.HasValue
            ? conversation.IsOwnedBy(userId.Value)
            : guestId != null && conversation.IsOwnedByGuest(guestId);

        if (!isOwner)
            throw new UnauthorizedAccessException(
                "You don't own this conversation");

        var messages = await _messageRepository
            .GetByConversationIdAsync(conversation.Id, ct);

        return new ConversationDetailDto
        {
            SessionId = conversation.AgentConversationId,
            Title = conversation.Title,
            MessageCount = conversation.MessageCount,
            CreatedAt = conversation.CreatedAt,
            UpdatedAt = conversation.UpdatedAt,
            LastMessageAt = conversation.LastMessageAt,
            Messages = messages.Select(m => new ChatMessageDto
            {
                Role = m.Role,
                Text = m.Text,
                CreatedAt = m.CreatedAt
            }).ToList()
        };
    }

    public async Task ChangeTitleOfConversationByIdAsync(
        int? userId,
        string? guestId,
        string conversationId,
        string newTitle,
        CancellationToken ct = default)
    {
        // ✅ 1. Валидация входных данных
        if (string.IsNullOrWhiteSpace(conversationId))
            throw new DomainException("SessionId is required");

        if (string.IsNullOrWhiteSpace(newTitle))
            throw new DomainException("NewTitle can't be empty or null");

        if (newTitle.Length > 200)
            throw new DomainException("NewTitle is too long (max 200)");

        // ✅ 2. Загружаем диалог
        var conversation = await _conversationRepository
            .GetByConversationIdAsync(conversationId, ct)
            ?? throw new DomainException($"Session {conversationId} not found");

        // ✅ 3. Проверка владельца
        var isOwner = userId.HasValue
            ? conversation.IsOwnedBy(userId.Value)
            : !string.IsNullOrWhiteSpace(guestId)
              && conversation.IsOwnedByGuest(guestId);

        if (!isOwner)
        {
            _logger.LogWarning(
                "Unauthorized title change attempt for session {SessionId} by {Owner}",
                conversationId,
                userId.HasValue ? $"user {userId}" : $"guest {guestId}");

            throw new UnauthorizedAccessException(
                "You don't own this conversation");
        }

        // ✅ 4. Проверка истечения (для гостей)
        if (conversation.IsExpired(_options.GuestConversationRetentionDays))
            throw new DomainException(
                "This conversation has expired");

        // ✅ 5. Меняем заголовок
        conversation.SetTitle(newTitle);

        // ✅ 6. Сохраняем
        await _unitOfWork.SaveChangesAsync(ct);

        // ✅ 7. Логируем
        _logger.LogInformation(
            "Conversation {SessionId} title changed to '{Title}' by {Owner}",
            conversationId,
            newTitle,
            userId.HasValue ? $"user {userId}" : $"guest {guestId}");
    }

    // ═══════════════════════════════════════════
    // User
    // ═══════════════════════════════════════════

    public async Task<bool> DeleteUserConversationAsync(
        int userId,
        string conversationId,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(conversationId))
            throw new DomainException("ConversationId is required");

        // ✅ Чёткая логика: только user
        var deleted = await _conversationRepository
            .DeleteByUserAndConversationIdAsync(
                userId, conversationId, ct);

        if (!deleted)
        {
            _logger.LogWarning(
                "Conversation {ConversationId} not found or not owned by user {UserId}",
                conversationId, userId);
            return false;
        }

        _logger.LogInformation(
            "Conversation {ConversationId} deleted by user {UserId}",
            conversationId, userId);

        return true;
    }

    // ═══════════════════════════════════════════
    // Guest
    // ═══════════════════════════════════════════

    public async Task<bool> DeleteGuestConversationAsync(
        string guestId,
        string conversationId,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(conversationId))
            throw new DomainException("ConversationId is required");

        if (string.IsNullOrWhiteSpace(guestId))
            throw new DomainException("GuestId is required");

        // ✅ Чёткая логика: только guest
        var deleted = await _conversationRepository
            .DeleteByGuestAndConversationIdAsync(
                guestId, conversationId, ct);

        if (!deleted)
        {
            _logger.LogWarning(
                "Conversation {ConversationId} not found or not owned by guest {GuestId}",
                conversationId, guestId);
            return false;
        }

        _logger.LogInformation(
            "Conversation {ConversationId} deleted by guest {GuestId}",
            conversationId, guestId);

        return true;
    }
}