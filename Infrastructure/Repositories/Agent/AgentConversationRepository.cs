using Application.Interfaces.Agent;
using Domain.Entities.Agent;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Agent;


public class AgentConversationRepository : IAgentConversationRepository
{
    private readonly AppDbContext _context;

    public AgentConversationRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<AgentConversationEntity?> GetByConversationIdAsync(
        string sessionId, CancellationToken ct = default)
    {
        return await _context.AgentConversations
            .FirstOrDefaultAsync(c => c.AgentConversationId == sessionId, ct);
    }

    public async Task<List<AgentConversationEntity>> 
        GetUserConversationsAsync(int userId, CancellationToken ct = default)
    {
        return await _context.AgentConversations.
            Where(c => c.UserId == userId)
            .ToListAsync();
    }

    public async Task<List<AgentConversationEntity>> GetGuestConversationsAsync(
    string guestId, CancellationToken ct = default)
    {
        return await _context.AgentConversations
            .AsNoTracking()
            .Where(c => c.GuestId == guestId && c.UserId == null)
            .OrderByDescending(c => c.UpdatedAt)
            .ToListAsync(ct);
    }

    public async Task<List<AgentConversationEntity>> GetByUserIdAsync(
        int userId, CancellationToken ct = default)
    {
        return await _context.AgentConversations
            .AsNoTracking()
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.UpdatedAt)
            .ToListAsync(ct);
    }

    public async Task AddAsync(
        AgentConversationEntity conversation, CancellationToken ct = default)
    {
        await _context.AgentConversations.AddAsync(conversation, ct);
    }

    public async Task<bool> DeleteByUserAndConversationIdAsync(
        int userId,
        string conversationId,
        CancellationToken ct = default)
    {
        // ✅ Чёткий WHERE: UserId AND ConversationId
        var deleted = await _context.AgentConversations
            .Where(c => c.UserId == userId)
            .Where(c => c.AgentConversationId == conversationId)
            .ExecuteDeleteAsync(ct);

        return deleted > 0;
    }

    public async Task<bool> DeleteByGuestAndConversationIdAsync(
        string guestId,
        string conversationId,
        CancellationToken ct = default)
    {
        // ✅ Чёткий WHERE: GuestId AND UserId IS NULL AND ConversationId
        var deleted = await _context.AgentConversations
            .Where(c => c.GuestId == guestId)
            .Where(c => c.UserId == null)  // ← защита
            .Where(c => c.AgentConversationId == conversationId)
            .ExecuteDeleteAsync(ct);

        return deleted > 0;
    }

    // ✅ Привязка гостевых диалогов при логине
    public async Task<int> AttachGuestConversationsToUserAsync(
        string guestId, int userId, CancellationToken ct = default)
    {
        return await _context.AgentConversations
            .Where(c => c.GuestId == guestId && c.UserId == null)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(c => c.UserId, userId)
                .SetProperty(c => c.GuestId, (string?)null)
                .SetProperty(c => c.UpdatedAt, DateTime.UtcNow), ct);
    }

    // ✅ Cleanup истёкших гостевых диалогов
    public async Task<int> DeleteExpiredAsync(
        int retentionDays, CancellationToken ct = default)
    {
        var threshold = DateTime.UtcNow.AddDays(-retentionDays);

        // ✅ Удаляем только гостевые диалоги, где не было активности > N дней
        return await _context.AgentConversations
            .Where(c => c.UserId == null)  // ← только гости
            .Where(c => (c.LastMessageAt ?? c.CreatedAt) < threshold)
            .ExecuteDeleteAsync(ct);
    }
}
