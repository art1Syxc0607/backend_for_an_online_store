using Application.Commands.Product;
using Application.DTOs.File;
using Application.Interfaces;
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
    public async Task Handle_WhenValid_ShouldAddProductAndInvalidateCache()
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
        var cacheServiceMock = new Mock<ICacheService>();
        var logger = new Mock<ILogger<AddProductCommandHandler>>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();

        var handler = new AddProductCommandHandler(
            productRepoMock.Object,
            categoryRepoMock.Object,
            fileStorageMock.Object,
            cacheServiceMock.Object,
            logger.Object,
            unitOfWorkMock.Object
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);


        // Assert
        result.Should().BeGreaterThan(0);
        productRepoMock.Verify(x => x.AddProductAsync(It.IsAny<Domain.Entities.Product>(), It.IsAny<CancellationToken>()), Times.Once);
        unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        cacheServiceMock.Verify(x => x.RemoveByPrefix("products:"), Times.Once);
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
            Mock.Of<ICacheService>(),
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
            Mock.Of<ICacheService>(),
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
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
