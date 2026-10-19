// InfrastructureTests/Services/UploadProductImageHandlerTests.cs
using Application.Commands.Product;
using Application.DTOs.Product;
using Application.DTOs.File;
using Application.Interfaces;
using Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System.Threading;
using Xunit;

namespace InfrastructureTests.Services;

public class UploadProductImageHandlerTests
{
    [Fact]
    public async Task Handle_WhenProductExists_ShouldUploadImageAndUpdateProduct()
    {
        // Arrange
        var productId = 1;
        var product = new Product("iPhone", 999.99m, 750m, 10, "Test");
        var imageUrl = "/images/products/1/test.jpg";

        var productRepoMock = new Mock<IProductRepository>();
        productRepoMock.Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        var fileStorageMock = new Mock<IFileStorageService>();
        fileStorageMock.Setup(x => x.UploadFileAsync(
            It.IsAny<Stream>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(imageUrl);

        var unitOfWorkMock = new Mock<IUnitOfWork>();

        var handler = new UploadProductFilesHandler(
            productRepoMock.Object,
            fileStorageMock.Object,
            unitOfWorkMock.Object,
            NullLogger<UploadProductFilesHandler>.Instance
        );

        var command = new UploadProductFilesCommand
        {
            ProductId = productId,

            Files = new List<Application.DTOs.File.FileUploadDto>
            {
                new Application.DTOs.File.FileUploadDto
                {
                    Stream = new MemoryStream(),
                    FileName = "test.jpg",
                    ContentType = "image/jpeg"
                }
            }

        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Contain(fdto => fdto.FileUrl == imageUrl);
        product.ImageUrls.Should().Contain(imageUrl);
        unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenProductDoesNotExist_ShouldThrowDomainException()
    {
        // Arrange
        var productRepoMock = new Mock<IProductRepository>();
        productRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        var handler = new UploadProductFilesHandler(
            productRepoMock.Object,
            Mock.Of<IFileStorageService>(),
            Mock.Of<IUnitOfWork>(),
            NullLogger<UploadProductFilesHandler>.Instance
        );

        var command = new UploadProductFilesCommand
        {
            ProductId = 999,
            Files = new List<Application.DTOs.File.FileUploadDto>
            {
                new Application.DTOs.File.FileUploadDto
                {
                    Stream = new MemoryStream(),
                    FileName = "test.jpg",
                    ContentType = "image/jpeg"
                }
            }
        };

        // Act
        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Domain.Exceptions.DomainException>()
            .WithMessage("*Product with ID 999 not found*");
    }

    [Fact]
    public async Task Handle_WhenSaveChangesFails_ShouldDeleteUploadedFiles()
    {
        // Arrange
        var productId = 1;
        var product = new Product("iPhone", 999.99m, 750m, 10, "Test");
        var imageUrl = "/images/products/1/test.jpg";

        var productRepoMock = new Mock<IProductRepository>();
        productRepoMock.Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        var fileStorageMock = new Mock<IFileStorageService>();
        fileStorageMock.Setup(x => x.UploadFileAsync(
            It.IsAny<Stream>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(imageUrl);

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("DB is down"));

        var handler = new UploadProductFilesHandler(
            productRepoMock.Object,
            fileStorageMock.Object,
            unitOfWorkMock.Object,
            NullLogger<UploadProductFilesHandler>.Instance
        );

        var command = new UploadProductFilesCommand
        {
            ProductId = productId,
            Files = new List<Application.DTOs.File.FileUploadDto>
            {
                new Application.DTOs.File.FileUploadDto
                {
                    Stream = new MemoryStream(),
                    FileName = "test.jpg",
                    ContentType = "image/jpeg"
                }
            }
        };

        // Act
        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
        fileStorageMock.Verify(x => x.DeleteMultipleFilesAsync(
            It.Is<List<string>>(l => l.Contains(imageUrl)),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    //Сводка теста: всего: 2; сбой: 0; успешно: 2; пропущено: 0; длительность: 2,3 с
}