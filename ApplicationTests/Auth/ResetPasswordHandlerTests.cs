using Application.Commands.User;
using Application.Interfaces;
using Domain.Entities;
using Domain.Exceptions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ApplicationTests.Auth;

public class ResetPasswordHandlerTests
{
    private static User CreateUser()
    {
        var user = new User("user@test.com", "old-hash", "User");
        user.TestsSetUser(id: 7);
        return user;
    }

    private static ResetPasswordCommandHandler CreateHandler(
        Mock<IUserRepository> userRepository,
        Mock<IPasswordResetRepository> resetRepository,
        Mock<IPasswordResetService> resetService,
        Mock<IPasswordHasher> passwordHasher,
        Mock<IUnitOfWork> unitOfWork)
    {
        return new ResetPasswordCommandHandler(
            userRepository.Object,
            resetRepository.Object,
            resetService.Object,
            passwordHasher.Object,
            unitOfWork.Object,
            Mock.Of<ILogger<ResetPasswordCommandHandler>>());
    }

    private static ResetPasswordCommand CreateCommand()
    {
        return new ResetPasswordCommand
        {
            Email = "user@test.com",
            Code = "123456",
            NewPassword = "NewPassword1!",
            ConfirmPassword = "NewPassword1!"
        };
    }

    [Fact]
    public async Task Handle_WhenCodeAlreadyUsed_ShouldRejectWithoutOpeningTransaction()
    {
        // Arrange
        var user = CreateUser();
        var usedCode = new PasswordResetCode(user.Id, "hash");
        usedCode.MarkAsUsed();

        var userRepository = new Mock<IUserRepository>();
        userRepository
            .Setup(x => x.GetByEmailAsync("user@test.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var resetRepository = new Mock<IPasswordResetRepository>();
        resetRepository
            .Setup(x => x.GetLatestByUserIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(usedCode);

        var unitOfWork = new Mock<IUnitOfWork>();

        var handler = CreateHandler(
            userRepository,
            resetRepository,
            new Mock<IPasswordResetService>(),
            new Mock<IPasswordHasher>(),
            unitOfWork);

        // Act
        Func<Task> act = async () => await handler.Handle(CreateCommand(), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("Invalid or expired reset code");

        unitOfWork.Verify(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
        unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenCodeIsValid_ShouldChangePasswordAndMarkCodeUsed()
    {
        // Arrange
        var user = CreateUser();
        var activeCode = new PasswordResetCode(user.Id, "hash");
        var lockedCode = new PasswordResetCode(user.Id, "hash");

        var userRepository = new Mock<IUserRepository>();
        userRepository
            .Setup(x => x.GetByEmailAsync("user@test.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var resetRepository = new Mock<IPasswordResetRepository>();
        resetRepository
            .Setup(x => x.GetLatestByUserIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(activeCode);
        resetRepository
            .Setup(x => x.GetLatestActiveForUpdateAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(lockedCode);

        var resetService = new Mock<IPasswordResetService>();
        resetService
            .Setup(x => x.VerifyCode("123456", "hash"))
            .Returns(true);

        var passwordHasher = new Mock<IPasswordHasher>();
        passwordHasher
            .Setup(x => x.HashPassword("NewPassword1!"))
            .Returns("new-hash");

        var unitOfWork = new Mock<IUnitOfWork>();

        var handler = CreateHandler(
            userRepository, resetRepository, resetService, passwordHasher, unitOfWork);

        // Act
        await handler.Handle(CreateCommand(), CancellationToken.None);

        // Assert
        lockedCode.IsUsed.Should().BeTrue();
        passwordHasher.Verify(x => x.HashPassword("NewPassword1!"), Times.Once);

        unitOfWork.Verify(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        unitOfWork.Verify(x => x.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        unitOfWork.Verify(x => x.RollbackTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenCodeWasConsumedByParallelRequest_ShouldRollbackAndReject()
    {
        // Arrange
        var user = CreateUser();
        var activeCode = new PasswordResetCode(user.Id, "hash");

        var userRepository = new Mock<IUserRepository>();
        userRepository
            .Setup(x => x.GetByEmailAsync("user@test.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var resetRepository = new Mock<IPasswordResetRepository>();
        resetRepository
            .Setup(x => x.GetLatestByUserIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(activeCode);
        // The parallel request already redeemed the code: re-read finds nothing.
        resetRepository
            .Setup(x => x.GetLatestActiveForUpdateAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PasswordResetCode?)null);

        var resetService = new Mock<IPasswordResetService>();
        resetService
            .Setup(x => x.VerifyCode("123456", "hash"))
            .Returns(true);

        var unitOfWork = new Mock<IUnitOfWork>();

        var handler = CreateHandler(
            userRepository,
            resetRepository,
            resetService,
            new Mock<IPasswordHasher>(),
            unitOfWork);

        // Act
        Func<Task> act = async () => await handler.Handle(CreateCommand(), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainException>();

        unitOfWork.Verify(x => x.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
        unitOfWork.Verify(x => x.RollbackTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenCodeIsWrong_ShouldIncrementAttemptsAtomically()
    {
        // Arrange
        var user = CreateUser();
        var activeCode = new PasswordResetCode(user.Id, "hash");

        var userRepository = new Mock<IUserRepository>();
        userRepository
            .Setup(x => x.GetByEmailAsync("user@test.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var resetRepository = new Mock<IPasswordResetRepository>();
        resetRepository
            .Setup(x => x.GetLatestByUserIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(activeCode);

        var resetService = new Mock<IPasswordResetService>();
        resetService
            .Setup(x => x.VerifyCode(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(false);

        var unitOfWork = new Mock<IUnitOfWork>();
        var handler = CreateHandler(
            userRepository,
            resetRepository,
            resetService,
            new Mock<IPasswordHasher>(),
            unitOfWork);

        // Act
        Func<Task> act = async () => await handler.Handle(CreateCommand(), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainException>();

        resetRepository.Verify(
            x => x.IncrementAttemptsAsync(activeCode.Id, It.IsAny<CancellationToken>()),
            Times.Once);

        unitOfWork.Verify(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
