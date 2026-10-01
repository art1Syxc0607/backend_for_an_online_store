using Domain.Entities.Agent;

namespace Application.Interfaces.Agent;

// Domain/Interfaces/IAgentMessageRepository.cs
public interface IAgentMessageRepository
{
    Task<List<AgentMessage>> GetByConversationIdAsync(
        int conversationId,
        CancellationToken ct = default);

    Task AddAsync(AgentMessage message, CancellationToken ct = default);
}
