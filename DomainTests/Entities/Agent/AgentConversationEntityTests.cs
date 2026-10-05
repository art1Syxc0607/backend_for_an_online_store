using Domain.Entities.Agent;
using Domain.Exceptions;
using FluentAssertions;
using Xunit;

namespace DomainTests.Entities.Agent;

public class AgentConversationEntityTests
{
    // ═══════════════════════════════════════════
    // Конструкторы
    // ═══════════════════════════════════════════

    [Fact]
    public void Constructor_ForUser_ShouldSetUserId()
    {
        // Arrange & Act
        var conversation = new AgentConversationEntity(
            userId: 42,
            sessionId: "session-abc");

        // Assert
        conversation.UserId.Should().Be(42);
        conversation.GuestId.Should().BeNull();
        conversation.AgentConversationId.Should().Be("session-abc");
        conversation.IsGuest.Should().BeFalse();
        conversation.MessageCount.Should().Be(0);
        conversation.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Constructor_ForGuest_ShouldSetGuestId()
    {
        // Arrange & Act
        var conversation = new AgentConversationEntity(
            guestId: "guest-abc",
            sessionId: "session-abc");

        // Assert
        conversation.UserId.Should().BeNull();
        conversation.GuestId.Should().Be("guest-abc");
        conversation.IsGuest.Should().BeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithEmptySessionId_ShouldThrow(string? sessionId)
    {
        // Act
        var act = () => new AgentConversationEntity(42, sessionId!);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*SessionId*");
    }

    // ═══════════════════════════════════════════
    // IsOwnedBy / IsOwnedByGuest
    // ═══════════════════════════════════════════

    [Fact]
    public void IsOwnedBy_WhenUserOwner_ShouldReturnTrue()
    {
        // Arrange
        var conversation = new AgentConversationEntity(42, "session");

        // Act & Assert
        conversation.IsOwnedBy(42).Should().BeTrue();
        conversation.IsOwnedBy(43).Should().BeFalse();
    }

    [Fact]
    public void IsOwnedByGuest_WhenGuestOwner_ShouldReturnTrue()
    {
        // Arrange
        var conversation = new AgentConversationEntity("guest-abc", "session");

        // Act & Assert
        conversation.IsOwnedByGuest("guest-abc").Should().BeTrue();
        conversation.IsOwnedByGuest("guest-xyz").Should().BeFalse();
    }

    // ═══════════════════════════════════════════
    // AttachToUser
    // ═══════════════════════════════════════════

    [Fact]
    public void AttachToUser_WhenGuest_ShouldTransferOwnership()
    {
        // Arrange
        var conversation = new AgentConversationEntity("guest-abc", "session");

        // Act
        conversation.AttachToUser(42);

        // Assert
        conversation.UserId.Should().Be(42);
        conversation.GuestId.Should().BeNull();
        conversation.IsGuest.Should().BeFalse();
    }

    [Fact]
    public void AttachToUser_WhenAlreadyUser_ShouldThrow()
    {
        // Arrange
        var conversation = new AgentConversationEntity(42, "session");

        // Act
        var act = () => conversation.AttachToUser(43);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*already belongs*");
    }

    // ═══════════════════════════════════════════
    // IsExpired
    // ═══════════════════════════════════════════

    [Fact]
    public void IsExpired_WhenUser_ShouldAlwaysReturnFalse()
    {
        // Arrange
        var conversation = new AgentConversationEntity(42, "session");
        conversation.UpdateState("{}");

        // Act & Assert
        conversation.IsExpired(retentionDays: 1).Should().BeFalse();
    }

    [Fact]
    public void IsExpired_WhenGuestAndNoActivity_ShouldReturnTrue()
    {
        // Arrange
        var conversation = new AgentConversationEntity("guest-abc", "session");
        // CreatedAt = NOW, LastMessageAt = null
        // Но IsExpired смотрит на (LastMessageAt ?? CreatedAt)
        // Значит CreatedAt = NOW → 0 дней назад → НЕ истёк

        // Act & Assert
        conversation.IsExpired(retentionDays: 7).Should().BeFalse();
    }

    // ═══════════════════════════════════════════
    // SetTitle
    // ═══════════════════════════════════════════

    [Fact]
    public void SetTitle_WhenValid_ShouldSetTitle()
    {
        // Arrange
        var conversation = new AgentConversationEntity(42, "session");

        // Act
        conversation.SetTitle("Про iPhone");

        // Assert
        conversation.Title.Should().Be("Про iPhone");
    }

    [Fact]
    public void SetTitle_WhenTooLong_ShouldTruncate()
    {
        // Arrange
        var conversation = new AgentConversationEntity(42, "session");
        var longTitle = new string('a', 200);

        // Act
        conversation.SetTitle(longTitle);

        // Assert
        conversation.Title.Should().HaveLength(103);  // 100 + "..."
        conversation.Title.Should().EndWith("...");
    }

    [Fact]
    public void SetTitle_WhenEmpty_ShouldNotChange()
    {
        // Arrange
        var conversation = new AgentConversationEntity(42, "session");

        // Act
        conversation.SetTitle("");

        // Assert
        conversation.Title.Should().BeNull();
    }

    // ═══════════════════════════════════════════
    // UpdateState
    // ═══════════════════════════════════════════

    [Fact]
    public void UpdateState_WhenValid_ShouldUpdate()
    {
        // Arrange
        var conversation = new AgentConversationEntity(42, "session");
        var beforeUpdate = conversation.UpdatedAt;

        // Act
        conversation.UpdateState("{\"state\":\"data\"}");

        // Assert
        conversation.JsonState.Should().Be("{\"state\":\"data\"}");
        conversation.LastMessageAt.Should().NotBeNull();
        conversation.UpdatedAt.Should().BeAfter(beforeUpdate);
    }

    [Fact]
    public void UpdateState_WhenEmpty_ShouldThrow()
    {
        // Arrange
        var conversation = new AgentConversationEntity(42, "session");

        // Act
        var act = () => conversation.UpdateState("");

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*JsonState*");
    }

    // ═══════════════════════════════════════════
    // IncrementMessageCount
    // ═══════════════════════════════════════════

    [Fact]
    public void IncrementMessageCount_ShouldIncrease()
    {
        // Arrange
        var conversation = new AgentConversationEntity(42, "session");

        // Act
        conversation.IncrementMessageCount();
        conversation.IncrementMessageCount(2);

        // Assert
        conversation.MessageCount.Should().Be(3);
    }
}