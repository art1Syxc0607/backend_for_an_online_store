// ApplicationTests/Commands/Auth/LoginHandlerTests.cs
using Application.Commands.User;
using Application.Interfaces;
using Domain.Entities;
using Domain.Exceptions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ApplicationTests.Commands.Auth;

public class LoginHandlerTests
{
    [Fact]
    public async Task Handle_WhenValidCredentials_ShouldReturnToken()
    {
        // Arrange
        var user = new User("test@mail.com", "hashedPassword", "JohnDoe");
        var command = new LoginCommand
        {
            Email = "test@mail.com",
            Password = "Password123!"
        };

        var userRepoMock = new Mock<IUserRepository>();
        userRepoMock.Setup(x => x.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var passwordHasherMock = new Mock<IPasswordHasher>();
        passwordHasherMock.Setup(x => x.VerifyPassword(command.Password, user.PasswordHash))
            .Returns(true);

        var jwtServiceMock = new Mock<IJwtService>();
        jwtServiceMock.Setup(x => x.GenerateToken(user))
            .Returns("jwtToken123");

        var handler = new LoginCommandHandler(
            userRepoMock.Object,
            passwordHasherMock.Object,
            jwtServiceMock.Object,
            Mock.Of<ILogger<LoginCommandHandler>>()
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Token.Should().Be("jwtToken123");
        result.Email.Should().Be(user.Email);
        result.UserName.Should().Be(user.UserName);
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldThrowDomainException()
    {
        // Arrange
        var command = new LoginCommand
        {
            Email = "test@mail.com",
            Password = "Password123!"
        };

        var userRepoMock = new Mock<IUserRepository>();
        userRepoMock.Setup(x => x.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var handler = new LoginCommandHandler(
            userRepoMock.Object,
            Mock.Of<IPasswordHasher>(),
            Mock.Of<IJwtService>(),
            Mock.Of<ILogger<LoginCommandHandler>>()
        );

        // Act
        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*Invalid email or password*");
    }

    [Fact]
    public async Task Handle_WhenPasswordIsWrong_ShouldThrowDomainException()
    {
        // Arrange
        var user = new User("test@mail.com", "hashedPassword", "JohnDoe");
        var command = new LoginCommand
        {
            Email = "test@mail.com",
            Password = "WrongPassword!"
        };

        var userRepoMock = new Mock<IUserRepository>();
        userRepoMock.Setup(x => x.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var passwordHasherMock = new Mock<IPasswordHasher>();
        passwordHasherMock.Setup(x => x.VerifyPassword(command.Password, user.PasswordHash))
            .Returns(false);

        var handler = new LoginCommandHandler(
            userRepoMock.Object,
            passwordHasherMock.Object,
            Mock.Of<IJwtService>(),
            Mock.Of<ILogger<LoginCommandHandler>>()
        );

        // Act
        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*Invalid email or password*");
    }
}