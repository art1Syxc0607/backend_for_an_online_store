using Domain.Entities.Agent;

namespace Application.Interfaces.Agent;

public interface IAgentConversationRepository
{
    Task<AgentConversationEntity?> GetByConversationIdAsync(
        string sessionId, CancellationToken ct = default);

    Task<List<AgentConversationEntity>> 
        GetUserConversationsAsync(int userId, bool desc = true, CancellationToken ct = default);
    Task<List<AgentConversationEntity>> GetGuestConversationsByGuestIdAsync(
        string guestId, bool desc = true, CancellationToken ct = default);
    //Task<List<AgentConversationEntity>> GetUserConversationsByUserIdAsync(
    //    int userId, CancellationToken ct = default);

    Task AddAsync(AgentConversationEntity conversation, CancellationToken ct = default);

    /// <summary>
    /// Удалить диалог пользователя.
    /// </summary>
    Task<bool> DeleteByUserAndConversationIdAsync(
        int userId,
        string conversationId,
        CancellationToken ct = default);

    /// <summary>
    /// Удалить диалог гостя.
    /// </summary>
    Task<bool> DeleteByGuestAndConversationIdAsync(
        string guestId,
        string conversationId,
        CancellationToken ct = default);

    Task<int> AttachGuestConversationsToUserAsync(
    string guestId, int userId, CancellationToken ct = default);

    Task<int> DeleteExpiredAsync(
        int retentionDays, CancellationToken ct = default);
}