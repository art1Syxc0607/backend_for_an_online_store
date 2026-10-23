using Application.Commands.Product;
using Application.DTOs.File;
using Application.Interfaces;
using Application.Interfaces.Caching;
using Castle.Core.Logging;
using Domain.Entities;
using Domain.Exceptions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ApplicationTests.Admin.Commands.Product;

public class AddProductAdminHandlerTests
{
    [Fact]
    public async Task Handle_WhenValid_ShouldAddProduct()
    {
        // Arrange
        var command = new AddProductCommand
        {
            Name = "iPhone 15",
            Price = 999.99m,
            PurchasePrice = 750.00m,
            StockQuantity = 10,
            Description = "Latest iPhone"
        };

        var productRepoMock = new Mock<IProductRepository>();
        productRepoMock.Setup(x => x.AddProductAsync(It.IsAny<Domain.Entities.Product>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Domain.Entities.Product p, CancellationToken ct) =>
            {
                p.TestsSetProduct(1);
                return p.Id;
            });
        var categoryRepoMock = new Mock<ICategoryRepository>();
        var fileStorageMock = new Mock<IFileStorageService>();
        var logger = new Mock<ILogger<AddProductCommandHandler>>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();

        var handler = new AddProductCommandHandler(
            productRepoMock.Object,
            categoryRepoMock.Object,
            fileStorageMock.Object,
            logger.Object,
            unitOfWorkMock.Object
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);


        // Assert
        result.Should().BeGreaterThan(0);
        productRepoMock.Verify(x => x.AddProductAsync(It.IsAny<Domain.Entities.Product>(), It.IsAny<CancellationToken>()), Times.Once);
        unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        // Инвалидацию выполняет CacheInvalidationBehavior по декларации команды
        command.CachePrefixesToInvalidate.Should().Contain(CacheKeys.ProductsPrefix);
    }

    [Fact]
    public async Task Handle_WhenCategoryNotFound_ShouldThrowDomainException()
    {
        // Arrange
        var command = new AddProductCommand
        {
            Name = "iPhone 15",
            Price = 999.99m,
            PurchasePrice = 750.00m,
            StockQuantity = 10,
            CategoryId = 999
        };

        var categoryRepoMock = new Mock<ICategoryRepository>();
        categoryRepoMock.Setup(x => x.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);

        var handler = new AddProductCommandHandler(
            Mock.Of<IProductRepository>(),
            categoryRepoMock.Object,
            Mock.Of<IFileStorageService>(),
            Mock.Of<ILogger<AddProductCommandHandler>>(),
            Mock.Of<IUnitOfWork>()
        );

        // Act
        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*Category 999 not found*");
    }

    [Fact]
    public async Task Handle_WhenWithFiles_ShouldUploadFiles()
    {
        // Arrange
        var command = new AddProductCommand
        {
            Name = "iPhone 15",
            Price = 999.99m,
            PurchasePrice = 750.00m,
            StockQuantity = 10,
            Description = "Latest iPhone",
            Files = new List<FileUploadDto>
            {
                new() { Stream = new MemoryStream(), FileName = "img1.jpg", ContentType = "image/jpeg" }
            }
        };

        var productRepoMock = new Mock<IProductRepository>();
        productRepoMock.Setup(x => x.AddProductAsync(It.IsAny<Domain.Entities.Product>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Domain.Entities.Product p, CancellationToken ct) =>
            {
                p.TestsSetProduct(1);
                return p.Id;
            });
        var categoryRepoMock = new Mock<ICategoryRepository>();
        var fileStorageMock = new Mock<IFileStorageService>();
        fileStorageMock.Setup(x => x.UploadFileAsync(
            It.IsAny<Stream>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync("/images/products/1/img1.jpg");

        var handler = new AddProductCommandHandler(
            productRepoMock.Object,
            categoryRepoMock.Object,
            fileStorageMock.Object,
            Mock.Of<ILogger<AddProductCommandHandler>>(),
            Mock.Of<IUnitOfWork>()
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeGreaterThan(0);
        fileStorageMock.Verify(x => x.UploadFileAsync(
            It.IsAny<Stream>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            "products/1",
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenMediaUploadFails_ShouldRollBackProductAndUploadedFiles()
    {
        // Arrange
        var command = new AddProductCommand
        {
            Name = "iPhone 15",
            Price = 999.99m,
            PurchasePrice = 750.00m,
            StockQuantity = 10,
            Description = "Latest iPhone",
            Files = new List<FileUploadDto>
            {
                new() { Stream = new MemoryStream(), FileName = "img1.jpg", ContentType = "image/jpeg" },
                new() { Stream = new MemoryStream(), FileName = "img2.jpg", ContentType = "image/jpeg" }
            }
        };

        var productRepoMock = new Mock<IProductRepository>();
        productRepoMock.Setup(x => x.AddProductAsync(It.IsAny<Domain.Entities.Product>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Domain.Entities.Product p, CancellationToken ct) =>
            {
                p.TestsSetProduct(1);
                return p.Id;
            });

        var fileStorageMock = new Mock<IFileStorageService>();
        fileStorageMock.SetupSequence(x => x.UploadFileAsync(
                It.IsAny<Stream>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync("/images/products/1/img1.jpg")
            .ThrowsAsync(new InvalidOperationException("upload failed"));

        var handler = new AddProductCommandHandler(
            productRepoMock.Object,
            Mock.Of<ICategoryRepository>(),
            fileStorageMock.Object,
            Mock.Of<ILogger<AddProductCommandHandler>>(),
            Mock.Of<IUnitOfWork>()
        );

        // Act
        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("upload failed");

        productRepoMock.Verify(x => x.DeleteProductAsync(
            It.IsAny<Domain.Entities.Product>(), It.IsAny<CancellationToken>()), Times.Once);

        fileStorageMock.Verify(x => x.DeleteMultipleFilesAsync(
            It.Is<List<string>>(urls => urls.Count == 1 && urls[0] == "/images/products/1/img1.jpg"),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
