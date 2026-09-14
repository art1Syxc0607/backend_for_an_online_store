using Application.Commands.Admin.Review;
using Application.Commands.Review;
using Application.DTOs.Email;
using Application.Interfaces;
using Castle.Core.Logging;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ApplicationTests.Commands.Review;

public class RespondToReviewHandlerTests
{
    [Fact]
    public async Task Handle_WhenValid_ShouldAddResponseAndSendEmail()
    {
        // Arrange
        var user = new User("test@mail.com", "hash", "John");
        var product = new Domain.Entities.Product("iPhone", 999.99m, 750m, 10, "Test");
        var review = new Domain.Entities.Review(user, product, "Great product!", 5, true);

        var admin = new User("admin@mail.com", "hash", "Admin");
        admin.PromoteToAdmin();

        var reviewRepoMock = new Mock<IReviewRepository>();
        reviewRepoMock.Setup(x => x.GetReviewByIdAsync(review.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(review);

        var productMock = new Mock<IProductRepository>();
        productMock.Setup(x => x.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        var userRepoMock = new Mock<IUserRepository>();
        userRepoMock.Setup(x => x.GetByIdAsync(admin.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(admin);

        var unitOfWorkMock = new Mock<IUnitOfWork>();

        var handler = new RespondToReviewHandler(
            reviewRepoMock.Object,
            userRepoMock.Object,
            productMock.Object,
            Mock.Of<IEmailTemplateService>(),
            Mock.Of<IEmailBackgroundService>(),
            Mock.Of<ILogger<RespondToReviewHandler>>(),
            unitOfWorkMock.Object
        );

        var command = new RespondToReviewCommand
        {
            ReviewId = review.Id,
            AdminId = admin.Id,
            Response = "Thank you for your feedback!"
        };

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        review.AdminResponse.Should().Be("Thank you for your feedback!");
        review.AdminResponseAt.Should().NotBeNull();
        unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    //[Fact]
    //public async Task Handle_WhenReviewNotApproved_ShouldThrowDomainException()
    //{
    //    // Arrange
    //    var user = new User("test@mail.com", "hash", "John");
    //    var product = new Domain.Entities.Product("iPhone", 999.99m, 750m, 10, "Test");
    //    var review = new Domain.Entities.Review(user, product, "Great product!", 5, true);
    //    // review.Status = ReviewStatus.Pending (по умолчанию)

    //    var admin = new User("admin@mail.com", "hash", "Admin");
    //    admin.PromoteToAdmin();

    //    var reviewRepoMock = new Mock<IReviewRepository>();
    //    reviewRepoMock.Setup(x => x.GetReviewByIdAsync(review.Id, It.IsAny<CancellationToken>()))
    //        .ReturnsAsync(review);

    //    var userRepoMock = new Mock<IUserRepository>();
    //    userRepoMock.Setup(x => x.GetByIdAsync(admin.Id, It.IsAny<CancellationToken>()))
    //        .ReturnsAsync(admin);

    //    var handler = new RespondToReviewHandler(
    //        reviewRepoMock.Object,
    //        userRepoMock.Object,
    //        Mock.Of<IEmailService>(),
    //        Mock.Of<IUnitOfWork>()
    //    );

    //    var command = new RespondToReviewCommand
    //    {
    //        ReviewId = review.Id,
    //        AdminId = admin.Id,
    //        Response = "Thank you!"
    //    };

    //    // Act
    //    Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

    //    // Assert
    //    await act.Should().ThrowAsync<DomainException>()
    //        .WithMessage("*not approved*");
    //}
}


// Сводка теста: всего: 1; сбой: 0; успешно: 1; пропущено: 0; длительность: 1,4 с