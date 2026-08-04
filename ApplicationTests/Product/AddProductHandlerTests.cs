// ApplicationTests/Commands/Product/AddProductHandlerTests.cs
using Application.Commands.Product;
using Application.Interfaces;
using Domain.Entities;
using Domain.Exceptions;
using FluentAssertions;
using MediatR;
using Moq;
using Xunit;

namespace ApplicationTests.Commands.Product;

public class AddProductHandlerTests
{
    [Fact]
    public async Task Handle_WhenValid_ShouldAddProduct()
    {
        // Arrange
        var command = new AddProductCommand
        {
            Name = "iPhone 15",
            Price = 999.99m,
            StockQuantity = 10,
            Description = "Latest iPhone"
        };

        var productRepoMock = new Mock<IProductRepository>();
        productRepoMock.Setup(x => x.AddProductAsync(It.IsAny<Domain.Entities.Product>(), It.IsAny<CancellationToken>()))
            .Callback<Domain.Entities.Product, CancellationToken>((product, ct) =>
            {
                product.TestsSetProduct(1);
            }).ReturnsAsync(1);
        var categoryRepoMock = new Mock<ICategoryRepository>();
        var fileStorageMock = new Mock<IFileStorageService>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();

        var handler = new AddProductCommandHandler(
            productRepoMock.Object,
            categoryRepoMock.Object,
            fileStorageMock.Object,
            Mock.Of<IMediator>(),
            unitOfWorkMock.Object
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeGreaterThan(0);
        productRepoMock.Verify(x => x.AddProductAsync(It.IsAny<Domain.Entities.Product>(), It.IsAny<CancellationToken>()), Times.Once);
        unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenCategoryNotFound_ShouldThrowDomainException()
    {
        // Arrange
        var command = new AddProductCommand
        {
            Name = "iPhone 15",
            Price = 999.99m,
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
            Mock.Of<IMediator>(),
            Mock.Of<IUnitOfWork>()
        );

        // Act
        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*Category 999 not found*");
    }
}