// ApplicationTests/Commands/Review/AddReviewHandlerTests.cs
using Application.Commands.Review;
using Application.Interfaces;
using Domain.Entities;
using Domain.Exceptions;
using FluentAssertions;
using Moq;
using Xunit;

namespace ApplicationTests.Commands.Review;

public class AddReviewHandlerTests
{
    [Fact]
    public async Task Handle_WhenValid_ShouldAddReview()
    {
        // Arrange
        var userId = 1;
        var productId = 1;
        var emailToken = "token123";
        var user = new User("test@mail.com", "hash", "John");
        user.GenerateEmailConfirmationToken(emailToken, DateTime.UtcNow + TimeSpan.FromMinutes(15));
        user.ConfirmEmail(emailToken);
        var product = new Domain.Entities.Product("iPhone", 999.99m, 10, "Test product");

        var userRepoMock = new Mock<IUserRepository>();
        userRepoMock.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var productRepoMock = new Mock<IProductRepository>();
        productRepoMock.Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        var orderRepoMock = new Mock<IOrderRepository>();
        orderRepoMock.Setup(x => x.HasUserPurchasedProductAsync(userId, productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var reviewRepoMock = new Mock<IReviewRepository>();
        reviewRepoMock.Setup(x => x.AddReviewAsync(It.IsAny<Domain.Entities.Review>(), It.IsAny<CancellationToken>()))
        .Callback<Domain.Entities.Review, CancellationToken>((review, ct) =>
        {
            review.TestsSetReviewId(1);
        }).Returns(Task.CompletedTask);

        var unitOfWorkMock = new Mock<IUnitOfWork>();

        var handler = new AddReviewCommandHandler(
            orderRepoMock.Object,
            unitOfWorkMock.Object,
            userRepoMock.Object,
            productRepoMock.Object,
            reviewRepoMock.Object
        );

        var command = new AddReviewCommand
        {
            UserId = userId,
            ProductId = productId,
            Text = "Great product!",
            Rating = 5
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeGreaterThan(0);
        reviewRepoMock.Verify(x => x.AddReviewAsync(It.IsAny<Domain.Entities.Review>(), It.IsAny<CancellationToken>()), Times.Once);
        unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserNotPurchasedProduct_ShouldThrowDomainException()
    {
        // Arrange
        var userId = 1;
        var productId = 1;
        var emailToken = "token123";
        var user = new User("test@mail.com", "hash", "John");
        user.GenerateEmailConfirmationToken(emailToken, DateTime.UtcNow + TimeSpan.FromMinutes(15));
        user.ConfirmEmail(emailToken);


        var userRepoMock = new Mock<IUserRepository>();
        userRepoMock.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var productRepoMock = new Mock<IProductRepository>();
        productRepoMock.Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Domain.Entities.Product("iPhone", 999.99m, 10, "Test product"));

        var orderRepoMock = new Mock<IOrderRepository>();
        orderRepoMock.Setup(x => x.HasUserPurchasedProductAsync(userId, productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false); // Не покупал!

        var handler = new AddReviewCommandHandler(
            orderRepoMock.Object,
            Mock.Of<IUnitOfWork>(),
            userRepoMock.Object,
            productRepoMock.Object,
            Mock.Of<IReviewRepository>()
        );

        var command = new AddReviewCommand
        {
            UserId = userId,
            ProductId = productId,
            Text = "Great product!",
            Rating = 5
        };

        // Act
        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*User didn't buy or recieved this product*");
    }
}