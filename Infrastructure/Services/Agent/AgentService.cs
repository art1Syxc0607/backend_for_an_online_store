using Application.DTOs.Agent;
using Application.Interfaces;
using Application.Interfaces.Agent;
using Domain.Entities.Agent;
using Domain.Exceptions;
using Infrastructure.Options;
using Microsoft.Agents.AI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
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

    // Семафоры на sessionId для защиты от параллельных запросов к одной сессии
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> _sessionLocks = new();

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

        // Определяем sessionId (генерируем для новых диалогов)
        string sessionId = request.SessionId ?? Guid.NewGuid().ToString();

        // 🔒 Защита от параллельных запросов к одной сессии.
        // Семафор гарантирует, что два запроса с одинаковым SessionId не будут
        // читать одно состояние JsonState, вызывать агента дважды и перезаписывать
        // изменения друг друга (lost update).
        var sem = _sessionLocks.GetOrAdd(sessionId, _ => new SemaphoreSlim(1, 1));
        await sem.WaitAsync(ct);
        try
        {
            return await ChatWithLockAsync(request, sessionId, ct);
        }
        finally
        {
            sem.Release();
        }
    }

    private async Task<ChatResponse> ChatWithLockAsync(
        ChatRequest request, string sessionId, CancellationToken ct)
    {
        AgentConversationEntity conversation;

        if (string.IsNullOrWhiteSpace(request.SessionId))
        {
            // Новый диалог
            conversation = request.UserId.HasValue
                ? new AgentConversationEntity(request.UserId.Value, sessionId)
                : new AgentConversationEntity(request.GuestId!, sessionId);

            conversation.SetTitle(request.Message);
            // Добавляем в контекст, сохранение произойдёт одним SaveChanges ниже.
            await _conversationRepository.AddAsync(conversation, ct);
        }
        else
        {
            conversation = await _conversationRepository
                .GetByConversationIdAsync(sessionId, ct)
                ?? throw new DomainException($"Session {sessionId} not found");

            // Проверка владельца
            var isOwner = request.UserId.HasValue
                ? conversation.IsOwnedBy(request.UserId.Value)
                : conversation.IsOwnedByGuest(request.GuestId!);

            if (!isOwner)
                throw new UnauthorizedAccessException("You don't own this conversation");

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
        var userMessage = AgentMessage.User(conversation, request.Message);
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

        // 4. Запускаем агента (внешний вызов — может упасть)
        string responseText;
        try
        {
            var response = await _agent.RunAsync(request.Message, agentSession);
            responseText = response.Text;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Agent RunAsync failed for session {SessionId}", sessionId);
            throw new DomainException("ИИ-помощник временно недоступен. Попробуйте позже.");
        }

        // 5. Сохраняем ответ ассистента
        var assistantMessage = AgentMessage.Assistant(conversation, responseText);
        await _messageRepository.AddAsync(assistantMessage, ct);

        // 6. Обновляем состояние (внутри UpdateState обновится LastMessageAt)
        var serializedSession = await _agent.SerializeSessionAsync(agentSession, JsonOptions, ct);
        conversation.UpdateState(serializedSession.GetRawText());
        conversation.IncrementMessageCount(2);

        // 7. Явная транзакция вокруг SaveChanges с оптимистической блокировкой xmin.
        // При конфликте (другой запрос уже обновил эту сессию) EF выбросит
        // DbUpdateConcurrencyException — мы откатываем транзакцию и кидаем
        // DomainException, который middleware мапит в 409 Conflict.
        await _unitOfWork.BeginTransactionAsync(ct);
        try
        {
            await _unitOfWork.SaveChangesAsync(ct);
            await _unitOfWork.CommitTransactionAsync(ct);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await _unitOfWork.RollbackTransactionAsync(ct);
            _logger.LogWarning(ex, "Concurrency conflict for session {SessionId}", sessionId);
            throw new DomainException("Разговор был обновлён другим запросом. Повторите попытку.");
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(ct);
            throw;
        }

        return new ChatResponse
        {
            Reply = responseText,
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
        var desc = true;
        var conversations = await _conversationRepository
            .GetUserConversationsAsync(userId, desc, ct);

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
        var desc = true;
        var conversations = await _conversationRepository
            .GetGuestConversationsByGuestIdAsync(guestId, desc, ct);

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