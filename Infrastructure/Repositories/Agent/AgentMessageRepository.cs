using Application.Interfaces.Agent;
using Domain.Entities.Agent;
using Infrastructure.Data;

namespace Infrastructure.Repositories.Agent;

// Infrastructure/Data/Repositories/AgentMessageRepository.cs
public class AgentMessageRepository : IAgentMessageRepository
{
    private readonly AppDbContext _context;

    public AgentMessageRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<AgentMessage>> GetByConversationIdAsync(
        int conversationId,
        CancellationToken ct = default)
    {
        return await _context.AgentMessages
            .AsNoTracking()
            .Where(m => m.ConversationId == conversationId)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task AddAsync(
        AgentMessage message,
        CancellationToken ct = default)
    {
        await _context.AgentMessages.AddAsync(message, ct);
    }
}
