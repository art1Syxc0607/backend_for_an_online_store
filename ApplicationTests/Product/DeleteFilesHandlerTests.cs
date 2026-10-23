using Application.Commands.Product;
using Application.Interfaces;
using Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace ApplicationTests.Commands.Product;

public class DeleteFilesHandlerTests
{
    [Fact]
    public async Task Handle_WhenDatabaseSaveFails_ShouldNotDeletePhysicalFile()
    {
        // Arrange
        const string fileUrl = "/images/products/1/photo.jpg";

        var product = new Domain.Entities.Product("iPhone", 999.99m, 900.99m, 10, "Test product");
        product.SetImageUrls(new List<string> { fileUrl });

        var productRepoMock = new Mock<IProductRepository>();
        productRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("db failed"));

        var fileStorageMock = new Mock<IFileStorageService>();

        var handler = new DeleteFilesHandler(
            productRepoMock.Object,
            fileStorageMock.Object,
            NullLogger<DeleteFilesHandler>.Instance,
            unitOfWorkMock.Object);

        var command = new DeleteFilesCommand
        {
            ProductId = 1,
            FileUrls = new List<string> { fileUrl }
        };

        // Act
        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("db failed");

        fileStorageMock.Verify(x => x.DeleteFileAsync(
            It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
