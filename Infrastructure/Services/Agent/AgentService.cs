// Infrastructure/Services/Agent/AgentService.cs
using Application.DTOs.Agent;
using Application.Interfaces;
using Application.Interfaces.Agent;
using Domain.Entities.Agent;
using Domain.Exceptions;
using Microsoft.Agents.AI;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Infrastructure.Services.Agent;

public class AgentService
{
    private readonly AIAgent _agent;
    private readonly IAgentMessageRepository _messageRepository;
    private readonly IAgentConversationRepository _conversationRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AgentService> _logger;

    public AgentService(
        AIAgent agent,
        IAgentConversationRepository conversationRepository,
        IUnitOfWork unitOfWork,
        ILogger<AgentService> logger)
    {
        _agent = agent;
        _conversationRepository = conversationRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<ChatResponse> ChatAsync(
        ChatRequest request,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
            throw new DomainException("Message is required");

        // 1. Находим или создаём диалог
        AgentConversationEntity conversation;

        if (string.IsNullOrWhiteSpace(request.SessionId))
        {
            conversation = new AgentConversationEntity(
                request.UserId,
                Guid.NewGuid().ToString());

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

            if (conversation.UserId != request.UserId)
                throw new UnauthorizedAccessException(
                    "You don't own this conversation");
        }

        // 2. ✅ Сохраняем сообщение пользователя
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
                conversation.JsonState, JsonSerializerOptions.Web);

            agentSession = await _agent.DeserializeSessionAsync(
                jsonElement,
                jsonSerializerOptions: JsonSerializerOptions.Web,
                cancellationToken: ct);
        }

        // 4. Запускаем агента
        var response = await _agent.RunAsync(request.Message, agentSession);

        // 5. ✅ Сохраняем ответ ассистента
        var assistantMessage = AgentMessage.Assistant(
            conversation.Id, response.Text);
        await _messageRepository.AddAsync(assistantMessage, ct);

        // 6. Обновляем состояние
        var serializedSession = await _agent.SerializeSessionAsync(
            agentSession,
            jsonSerializerOptions: JsonSerializerOptions.Web,
            cancellationToken: ct);

        conversation.UpdateState(serializedSession.GetRawText());
        await _unitOfWork.SaveChangesAsync(ct);

        return new ChatResponse
        {
            Reply = response.Text,
            SessionId = conversation.AgentConversationId,
            MessageCount = conversation.MessageCount
        };
    }
    public async Task<List<ConversationDto>> GetConversationsAsync(int userId, CancellationToken ct)
    {
        var conversations = await _conversationRepository.GetUserConversationsAsync(userId, ct);

        return conversations.Select(c => new ConversationDto
        {
            SessionId = c.AgentConversationId,
            MessageCount = c.MessageCount,
            Title = c.Title,
            CreatedAt = c.CreatedAt,
            UpdatedAt = c.UpdatedAt,
            LastMessageAt = c.LastMessageAt
        }).ToList();
    }

    public async Task<ConversationDetailDto> GetConversationAsync(
        int userId,
        string sessionId,
        CancellationToken ct = default)
    {
        // 1. Находим диалог
        var conversation = await _conversationRepository
            .GetByConversationIdAsync(sessionId, ct)
            ?? throw new DomainException(
                $"Session {sessionId} not found");

        // 2. Проверяем владельца
        if (conversation.UserId != userId)
            throw new UnauthorizedAccessException(
                "You don't own this conversation");

        // 3. ✅ Загружаем сообщения
        var messages = await _messageRepository
            .GetByConversationIdAsync(conversation.Id, ct);

        // 4. Формируем DTO
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

    public async Task DeleteUserConversationByIdAsync(int userId, string conversationId, CancellationToken ct)
    {
        var conversation = await _conversationRepository.GetByConversationIdAsync
            (conversationId, ct);

        if (conversation == null)
        {
            _logger.LogWarning("Conversation with Id {id} not found",
                conversationId);
            throw new DomainException
                ($"Conversation with Id {conversationId} not found");
        }
        if(conversation.UserId != userId)
        {
            _logger.LogWarning("You don't own this " +
                "conversation, Id {conversationId}",
                conversationId);
            throw new UnauthorizedAccessException(
                $"You don't own this conversation, Id {conversationId}");
        }


        await _conversationRepository.DeleteUserConversationByIdAsync(userId, conversationId, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Agent conversations with Id {convId} " +
            "cleared for user {UserId}", conversationId, userId);
    }
}