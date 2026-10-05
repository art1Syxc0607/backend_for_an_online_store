using Domain.Entities.Agent;
using Domain.Exceptions;
using FluentAssertions;
using Xunit;

namespace DomainTests.Entities.Agent;

public class AgentMessageTests
{
    [Fact]
    public void User_ShouldCreateUserMessage()
    {
        // Act
        var message = AgentMessage.User(conversationId: 1, text: "Hello");

        // Assert
        message.ConversationId.Should().Be(1);
        message.Role.Should().Be(Domain.Enums.AgentMessageRole.User);
        message.Text.Should().Be("Hello");
        message.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Assistant_ShouldCreateAssistantMessage()
    {
        // Act
        var message = AgentMessage.Assistant(conversationId: 1, text: "Hi!");

        // Assert
        message.Role.Should().Be(Domain.Enums.AgentMessageRole.Assistant);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void User_WhenEmptyText_ShouldThrow(string? text)
    {
        // Act
        var act = () => AgentMessage.User(1, text!);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*Text*");
    }
}