using Application.Commands.User;
using Application.Interfaces;
using Application.DTOs.Email;
using Domain.Entities;
using Domain.Exceptions;
using FluentAssertions;
using Moq;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace ApplicationTests.Commands.Auth;

public class RegisterHandlerTests
{
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<IJwtService> _jwtServiceMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<ITokenGenerator> _tokenGeneratorMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly RegisterHandler _handler;

    public RegisterHandlerTests()
    {
        _userRepoMock = new Mock<IUserRepository>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _jwtServiceMock = new Mock<IJwtService>();
        _emailServiceMock = new Mock<IEmailService>();
        _tokenGeneratorMock = new Mock<ITokenGenerator>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _handler = new RegisterHandler(
            _userRepoMock.Object,
            _passwordHasherMock.Object,
            _jwtServiceMock.Object,
            _emailServiceMock.Object,
            _tokenGeneratorMock.Object,
            Mock.Of<IConfiguration>(),
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task Handle_WhenValidData_ShouldRegisterUser()
    {
        // Arrange
        var command = new RegisterCommand
        {
            Email = "test@mail.com",
            UserName = "JohnDoe",
            Password = "Password123!"
        };

        _userRepoMock.Setup(x => x.ExistsByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _userRepoMock.Setup(x => x.ExistsByUserNameAsync(command.UserName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _passwordHasherMock.Setup(x => x.HashPassword(command.Password))
            .Returns("hashedPassword");
        _tokenGeneratorMock.Setup(x => x.GenerateEmailConfirmationToken())
            .Returns("token123");
        _jwtServiceMock.Setup(x => x.GenerateToken(It.IsAny<User>()))
            .Returns("jwtToken123");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Email.Should().Be(command.Email);
        result.UserName.Should().Be(command.UserName);
        result.Token.Should().Be("jwtToken123");
        result.IsEmailConfirmed.Should().BeFalse();

        _userRepoMock.Verify(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        //_emailServiceMock.Verify(x => x.SendEmailAsync(It.IsAny<EmailDto>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenEmailAlreadyExists_ShouldThrowDomainException()
    {
        // Arrange
        var command = new RegisterCommand
        {
            Email = "test@mail.com",
            UserName = "JohnDoe",
            Password = "Password123!"
        };

        _userRepoMock.Setup(x => x.ExistsByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*Email already registered*");

        _userRepoMock.Verify(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenUserNameAlreadyExists_ShouldThrowDomainException()
    {
        // Arrange
        var command = new RegisterCommand
        {
            Email = "test@mail.com",
            UserName = "JohnDoe",
            Password = "Password123!"
        };

        _userRepoMock.Setup(x => x.ExistsByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _userRepoMock.Setup(x => x.ExistsByUserNameAsync(command.UserName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*Username already taken*");
    }
}