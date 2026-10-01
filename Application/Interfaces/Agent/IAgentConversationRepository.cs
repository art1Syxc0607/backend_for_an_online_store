using Domain.Entities.Agent;

namespace Application.Interfaces.Agent;

public interface IAgentConversationRepository
{
    Task<AgentConversationEntity?> GetByConversationIdAsync(
        string sessionId, CancellationToken ct = default);

    Task<List<AgentConversationEntity>> GetByUserIdAsync(
        int userId, CancellationToken ct = default);

    Task AddAsync(AgentConversationEntity conversation, CancellationToken ct = default);

    Task DeleteByUserIdAsync(int userId, CancellationToken ct = default);

    Task<List<AgentConversationEntity>> GetUserConversationsAsync(int userId, CancellationToken ct = default);
}