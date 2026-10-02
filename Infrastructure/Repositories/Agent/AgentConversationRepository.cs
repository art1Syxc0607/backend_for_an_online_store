using Application.Interfaces.Agent;
using Domain.Entities.Agent;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

    public async Task DeleteUserConversationByIdAsync(
        int userId, string conversationId, CancellationToken ct = default)
    {
        await _context.AgentConversations
            .Where(c => c.AgentConversationId == conversationId)
            .ExecuteDeleteAsync(ct);
    }
}
