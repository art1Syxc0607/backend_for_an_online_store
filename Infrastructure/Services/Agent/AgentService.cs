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
            // Новый диалог
            conversation = new AgentConversationEntity(
                request.UserId,
                Guid.NewGuid().ToString());

            // ✅ Заголовок из первого сообщения
            conversation.SetTitle(request.Message);

            await _conversationRepository.AddAsync(conversation, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            _logger.LogInformation(
                "New agent conversation created: {SessionId} for user {UserId}",
                conversation.AgentConversationId, request.UserId);
        }
        else
        {
            // Существующий диалог
            conversation = await _conversationRepository
                .GetByConversationIdAsync(request.SessionId, ct)
                ?? throw new DomainException(
                    $"Session {request.SessionId} not found");

            // ✅ Проверяем, что диалог принадлежит пользователю
            if (conversation.UserId != request.UserId)
                throw new UnauthorizedAccessException(
                    "You don't own this conversation");
        }

        // 2. Восстанавливаем или создаём AgentSession
        AgentSession agentSession;

        if (string.IsNullOrWhiteSpace(conversation.JsonState))
        {
            // Первое сообщение — новая сессия
            agentSession = await _agent.CreateSessionAsync(ct);
        }
        else
        {
            // Восстанавливаем из JSON
            var jsonElement = JsonSerializer.Deserialize<JsonElement>(
                conversation.JsonState, JsonSerializerOptions.Web);

            agentSession = await _agent.DeserializeSessionAsync(
                jsonElement,
                jsonSerializerOptions: JsonSerializerOptions.Web,
                cancellationToken: ct);
        }

        // 3. Запускаем агента
        var response = await _agent.RunAsync(request.Message, agentSession);

        // 4. Сериализуем и сохраняем состояние
        var serializedSession = await _agent.SerializeSessionAsync(
            agentSession,
            jsonSerializerOptions: JsonSerializerOptions.Web,
            cancellationToken: ct);

        conversation.UpdateState(serializedSession.GetRawText());
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Agent responded in session {SessionId}, message #{Count}",
            conversation.AgentConversationId, conversation.MessageCount);

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

    public async Task<ConversationDetailDto> GetConversation(int userId, string agentConversationId, CancellationToken ct)
    {
        var conversation = await _conversationRepository.GetByConversationIdAsync(agentConversationId, ct);
        if(conversation == null)
        {
            _logger.LogWarning("Conversation with ID {agentConversationId} not found", agentConversationId);
            throw new DomainException($"Conversation with ID {agentConversationId} not found");
            
        }
        if(conversation.UserId != userId)
        {
            _logger.LogWarning("Conversation with ID {agentConversationId} doesn't belong to the user", agentConversationId);
            throw new DomainException($"Conversation with ID {agentConversationId} doesn't belong to the user");
        }

        return new ConversationDetailDto
        {
            SessionId = conversation.AgentConversationId,
            MessageCount = conversation.MessageCount,
            Title = conversation.Title,
            CreatedAt = conversation.CreatedAt,
            UpdatedAt = conversation.UpdatedAt,
            LastMessageAt = conversation.LastMessageAt,
            //Messages =  
        };
    }

    public async Task ClearSessionAsync(int userId, CancellationToken ct)
    {
        // ✅ Удаляем все диалоги пользователя
        await _conversationRepository.DeleteByUserIdAsync(userId, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation(
            "All agent conversations cleared for user {UserId}", userId);
    }
}