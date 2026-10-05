// InfrastructureTests/Repositories/Agent/AgentConversationRepositoryTests.cs
using Domain.Entities.Agent;
using FluentAssertions;
using Infrastructure.Data;
using Infrastructure.Repositories.Agent;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace InfrastructureTests.Repositories.Agent;

public class AgentConversationRepositoryTests : IAsyncLifetime
{
    private AppDbContext _context = null!;
    private AgentConversationRepository _repository = null!;

    public async Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"AgentTest_{Guid.NewGuid()}")
            .Options;

        _context = new AppDbContext(options);
        _repository = new AgentConversationRepository(_context);

        await Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        await _context.DisposeAsync();
    }

    [Fact]
    public async Task GetByConversationIdAsync_WhenExists_ReturnsConversation()
    {
        // Arrange
        var conversation = new AgentConversationEntity(42, "session-abc");
        await _context.AgentConversations.AddAsync(conversation);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByConversationIdAsync("session-abc");

        // Assert
        result.Should().NotBeNull();
        result!.UserId.Should().Be(42);
    }

    [Fact]
    public async Task GetByConversationIdAsync_WhenNotExists_ReturnsNull()
    {
        // Act
        var result = await _repository.GetByConversationIdAsync("nonexistent");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task AttachGuestConversationsToUserAsync_ShouldTransferAll()
    {
        // Arrange
        var guestId = "guest-abc";
        var userId = 42;

        var conv1 = new AgentConversationEntity(guestId, "s1");
        var conv2 = new AgentConversationEntity(guestId, "s2");

        await _context.AgentConversations.AddRangeAsync(conv1, conv2);
        await _context.SaveChangesAsync();

        // Act
        var count = await _repository.AttachGuestConversationsToUserAsync(
            guestId, userId);

        // Assert
        count.Should().Be(2);

        var updated = await _context.AgentConversations.ToListAsync();
        updated.Should().AllSatisfy(c =>
        {
            c.UserId.Should().Be(userId);
            c.GuestId.Should().BeNull();
        });
    }

    [Fact]
    public async Task DeleteExpiredAsync_ShouldDeleteOnlyInactive()
    {
        // Arrange
        var activeConv = new AgentConversationEntity("guest-1", "s1");
        activeConv.UpdateState("{}");  // ← LastMessageAt = NOW

        var expiredConv = new AgentConversationEntity("guest-2", "s2");
        // LastMessageAt = null, CreatedAt = NOW (не истёк)

        var veryOldConv = new AgentConversationEntity("guest-3", "s3");
        // Нужно симулировать CreatedAt = -10 дней

        var userConv = new AgentConversationEntity(42, "s4");
        userConv.UpdateState("{}");  // ← User — не удаляем

        await _context.AgentConversations.AddRangeAsync(
            activeConv, expiredConv, veryOldConv, userConv);
        await _context.SaveChangesAsync();

        // Act
        var deleted = await _repository.DeleteExpiredAsync(retentionDays: 7);

        // Assert
        // activeConv — остался (обновлён)
        // expiredConv — остался (CreatedAt = NOW)
        // veryOldConv — удалён (если CreatedAt старый)
        // userConv — остался (не гость)
    }
}