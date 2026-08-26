using Application.Commands.Admin.User;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ApplicationTests.Admin.Commands;

public class PromoteToAdminHandlerTests
{
    [Fact]
    public async Task Handle_WhenUserExists_ShouldPromoteToAdmin()
    {
        // Arrange
        var user = new User("test@mail.com", "hash", "John");
        user.TestsSetUser(1);
        var admin = new User("admin@mail.com", "hash", "Admin");
        admin.TestsSetUser(2);
        admin.PromoteToAdmin();

        var userRepoMock = new Mock<IUserRepository>();
        userRepoMock.Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        userRepoMock.Setup(x => x.GetByIdAsync(admin.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(admin);

        var unitOfWorkMock = new Mock<IUnitOfWork>();

        var handler = new PromoteToAdminHandler(
            userRepoMock.Object,
            Mock.Of<ILogger<PromoteToAdminHandler>>(),
            unitOfWorkMock.Object
        );

        var command = new PromoteToAdminCommand
        {
            UserId = user.Id,
            AdminId = admin.Id
        };

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        user.Role.Should().Be(UserRole.Admin);
        unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldThrowDomainException()
    {
        // Arrange
        var userRepoMock = new Mock<IUserRepository>();
        userRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var handler = new PromoteToAdminHandler(
            userRepoMock.Object,
            Mock.Of<ILogger<PromoteToAdminHandler>>(),
            Mock.Of<IUnitOfWork>()
        );

        var command = new PromoteToAdminCommand
        {
            UserId = 999,
            AdminId = 1
        };

        // Act
        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*User not found*");
    }

    [Fact]
    public async Task Handle_WhenUserIsAlreadyAdmin_ShouldThrowDomainException()
    {
        // Arrange
        var user = new User("test@mail.com", "hash", "John");
        user.PromoteToAdmin();
        var admin = new User("admin@mail.com", "hash", "Admin");
        admin.PromoteToAdmin();

        var userRepoMock = new Mock<IUserRepository>();
        userRepoMock.Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var handler = new PromoteToAdminHandler(
            userRepoMock.Object,
            Mock.Of<ILogger<PromoteToAdminHandler>>(),
            Mock.Of<IUnitOfWork>()
        );

        var command = new PromoteToAdminCommand
        {
            UserId = user.Id,
            AdminId = admin.Id
        };

        // Act
        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*already an admin*");
    }

    [Fact]
    public async Task Handle_WhenPromotingSelf_ShouldThrowDomainException()
    {
        // Arrange
        var admin = new User("admin@mail.com", "hash", "Admin");
        admin.PromoteToAdmin();

        var userRepoMock = new Mock<IUserRepository>();
        userRepoMock.Setup(x => x.GetByIdAsync(admin.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(admin);

        var handler = new PromoteToAdminHandler(
            userRepoMock.Object,
            Mock.Of<ILogger<PromoteToAdminHandler>>(),
            Mock.Of<IUnitOfWork>()
        );

        var command = new PromoteToAdminCommand
        {
            UserId = admin.Id,
            AdminId = admin.Id
        };

        // Act
        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*Already an admin*");
    }
}