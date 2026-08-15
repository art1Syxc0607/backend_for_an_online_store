//using Application.Commands.Admin.BlockUser;
//using Application.Interfaces;
//using Domain.Entities;
//using Domain.Enums;
//using Domain.Exceptions;
//using FluentAssertions;
//using Moq;
//using Xunit;

//namespace ApplicationTests.Commands.Admin;

//public class BlockUserHandlerTests
//{
//    [Fact]
//    public async Task Handle_WhenUserExistsAndNotAdmin_ShouldBlockUser()
//    {
//        // Arrange
//        var user = new User("test@mail.com", "hash", "John");
//        var admin = new User("admin@mail.com", "hash", "Admin");
//        admin.PromoteToAdmin();

//        var userRepoMock = new Mock<IUserRepository>();
//        userRepoMock.Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
//            .ReturnsAsync(user);

//        var handler = new BlockUserHandler(
//            userRepoMock.Object,
//            Mock.Of<IUnitOfWork>()
//        );

//        var command = new BlockUserCommand
//        {
//            UserId = user.Id,
//            AdminId = admin.Id,
//            Reason = "Spam"
//        };

//        // Act
//        await handler.Handle(command, CancellationToken.None);

//        // Assert
//        user.IsActive.Should().BeFalse();
//        user.BlockReason.Should().Be("Spam");
//        user.BlockedAt.Should().NotBeNull();
//    }

//    [Fact]
//    public async Task Handle_WhenUserIsAdmin_ShouldThrowDomainException()
//    {
//        // Arrange
//        var user = new User("admin@mail.com", "hash", "Admin");
//        user.PromoteToAdmin();
//        var admin = new User("admin2@mail.com", "hash", "Admin2");
//        admin.PromoteToAdmin();

//        var userRepoMock = new Mock<IUserRepository>();
//        userRepoMock.Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
//            .ReturnsAsync(user);

//        var handler = new BlockUserHandler(
//            userRepoMock.Object,
//            Mock.Of<IUnitOfWork>()
//        );

//        var command = new BlockUserCommand
//        {
//            UserId = user.Id,
//            AdminId = admin.Id
//        };

//        // Act
//        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

//        // Assert
//        await act.Should().ThrowAsync<DomainException>()
//            .WithMessage("*Cannot block an admin*");
//    }

//    [Fact]
//    public async Task Handle_WhenUserAlreadyBlocked_ShouldThrowDomainException()
//    {
//        // Arrange
//        var user = new User("test@mail.com", "hash", "John");
//        user.Block("Spam");
//        var admin = new User("admin@mail.com", "hash", "Admin");
//        admin.PromoteToAdmin();

//        var userRepoMock = new Mock<IUserRepository>();
//        userRepoMock.Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
//            .ReturnsAsync(user);

//        var handler = new BlockUserHandler(
//            userRepoMock.Object,
//            Mock.Of<IUnitOfWork>()
//        );

//        var command = new BlockUserCommand
//        {
//            UserId = user.Id,
//            AdminId = admin.Id
//        };

//        // Act
//        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

//        // Assert
//        await act.Should().ThrowAsync<DomainException>()
//            .WithMessage("*already blocked*");
//    }
//}