using Application.Commands.Review;
using Application.DTOs.File;
using Application.Interfaces;
using Domain.Entities;
using Domain.Exceptions;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace ApplicationTests.Review;

public class UploadReviewFilesHandlerTests
{
    [Fact]
    public async Task Handle_WhenReviewBelongsToAnotherUser_ShouldThrowDomainException()
    {
        // Arrange
        var owner = new User("owner@mail.com", "hash", "Owner") { Id = 10 };
        var product = new Domain.Entities.Product("iPhone", 999.99m, 900.99m, 10, "Test product");
        var review = new Domain.Entities.Review(owner, product, "Great product!", 5, true);
        review.TestsSetReviewId(42);

        var reviewRepoMock = new Mock<IReviewRepository>();
        reviewRepoMock.Setup(x => x.GetReviewByIdAsync(42, It.IsAny<CancellationToken>()))
            .ReturnsAsync(review);

        var handler = new UploadReviewFilesHandler(
            reviewRepoMock.Object,
            Mock.Of<IFileStorageService>(),
            NullLogger<UploadReviewFilesHandler>.Instance,
            Mock.Of<IUnitOfWork>());

        var command = new UploadReviewFilesCommand
        {
            ReviewId = 42,
            UserId = 11,
            Files = new List<FileUploadDto>
            {
                new() { Stream = new MemoryStream(), FileName = "img.jpg", ContentType = "image/jpeg" }
            }
        };

        // Act
        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*doesn't belong to the user*");
    }
}
