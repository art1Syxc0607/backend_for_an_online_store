//using Application.Queries.Product;
//using Application.Interfaces;
//using Domain.Entities;
//using Domain.Exceptions;
//using FluentAssertions;
//using Moq;
//using Xunit;

//namespace ApplicationTests.Queries.Product;

//public class GetProductHandlerTests
//{
//    [Fact]
//    public async Task Handle_WhenProductExists_ShouldReturnProductDto()
//    {
//        // Arrange
//        var productId = 1;
//        var product = new Product(
//            name: "iPhone 15",
//            price: 999.99m,
//            stockQuantity: 10,
//            description: "Latest iPhone model",
//            categoryId: 1,
//            id: productId
//        );

//        var productRepoMock = new Mock<IProductRepository>();
//        productRepoMock.Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
//            .ReturnsAsync(product);

//        var handler = new GetProductCommandHandler(productRepoMock.Object);
//        var query = new GetProductQuery { Id = productId };

//        // Act
//        var result = await handler.Handle(query, CancellationToken.None);

//        // Assert
//        result.Should().NotBeNull();
//        result.Id.Should().Be(productId);
//        result.Name.Should().Be("iPhone 15");
//        result.Price.Should().Be(999.99m);
//        result.StockQuantity.Should().Be(10);
//        result.Description.Should().Be("Latest iPhone model");
//        result.CategoryId.Should().Be(1);
//        productRepoMock.Verify(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()), Times.Once);
//    }

//    [Fact]
//    public async Task Handle_WhenProductNotFound_ShouldThrowDomainException()
//    {
//        // Arrange
//        var productId = 999;

//        var productRepoMock = new Mock<IProductRepository>();
//        productRepoMock.Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
//            .ReturnsAsync((Product?)null);

//        var handler = new GetProductHandler(productRepoMock.Object);
//        var query = new GetProductQuery { Id = productId };

//        // Act
//        Func<Task> act = async () => await handler.Handle(query, CancellationToken.None);

//        // Assert
//        await act.Should().ThrowAsync<DomainException>()
//            .WithMessage($"Product with ID {productId} not found");
//        productRepoMock.Verify(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()), Times.Once);
//    }

//    [Fact]
//    public async Task Handle_WhenProductHasNoImageUrls_ShouldReturnEmptyList()
//    {
//        // Arrange
//        var productId = 1;
//        var product = new Product(
//            name: "iPhone 15",
//            price: 999.99m,
//            stockQuantity: 10,
//            description: "Latest iPhone model",
//            categoryId: 1,
//            id: productId
//        );

//        var productRepoMock = new Mock<IProductRepository>();
//        productRepoMock.Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
//            .ReturnsAsync(product);

//        var handler = new GetProductHandler(productRepoMock.Object);
//        var query = new GetProductQuery { Id = productId };

//        // Act
//        var result = await handler.Handle(query, CancellationToken.None);

//        // Assert
//        result.Should().NotBeNull();
//        result.ImageUrls.Should().BeEmpty();
//        result.VideoUrls.Should().BeEmpty();
//    }

//    [Fact]
//    public async Task Handle_WhenProductHasImages_ShouldReturnImageUrls()
//    {
//        // Arrange
//        var productId = 1;
//        var product = new Product(
//            name: "iPhone 15",
//            price: 999.99m,
//            stockQuantity: 10,
//            description: "Latest iPhone model",
//            categoryId: 1,
//            id: productId
//        );
//        product.SetImageUrls(new List<string>
//        {
//            "https://example.com/image1.jpg",
//            "https://example.com/image2.jpg"
//        });

//        var productRepoMock = new Mock<IProductRepository>();
//        productRepoMock.Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
//            .ReturnsAsync(product);

//        var handler = new GetProductHandler(productRepoMock.Object);
//        var query = new GetProductQuery { Id = productId };

//        // Act
//        var result = await handler.Handle(query, CancellationToken.None);

//        // Assert
//        result.Should().NotBeNull();
//        result.ImageUrls.Should().HaveCount(2);
//        result.ImageUrls.Should().Contain("https://example.com/image1.jpg");
//        result.ImageUrls.Should().Contain("https://example.com/image2.jpg");
//    }

//    [Fact]
//    public async Task Handle_WhenProductHasVideos_ShouldReturnVideoUrls()
//    {
//        // Arrange
//        var productId = 1;
//        var product = new Product(
//            name: "iPhone 15",
//            price: 999.99m,
//            stockQuantity: 10,
//            description: "Latest iPhone model",
//            categoryId: 1,
//            id: productId
//        );
//        product.SetVideoUrls(new List<string>
//        {
//            "https://example.com/video1.mp4",
//            "https://example.com/video2.mp4"
//        });

//        var productRepoMock = new Mock<IProductRepository>();
//        productRepoMock.Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
//            .ReturnsAsync(product);

//        var handler = new GetProductHandler(productRepoMock.Object);
//        var query = new GetProductQuery { Id = productId };

//        // Act
//        var result = await handler.Handle(query, CancellationToken.None);

//        // Assert
//        result.Should().NotBeNull();
//        result.VideoUrls.Should().HaveCount(2);
//        result.VideoUrls.Should().Contain("https://example.com/video1.mp4");
//        result.VideoUrls.Should().Contain("https://example.com/video2.mp4");
//    }

//    [Fact]
//    public async Task Handle_WhenProductHasBothImagesAndVideos_ShouldReturnBoth()
//    {
//        // Arrange
//        var productId = 1;
//        var product = new Product(
//            name: "iPhone 15",
//            price: 999.99m,
//            stockQuantity: 10,
//            description: "Latest iPhone model",
//            categoryId: 1,
//            id: productId
//        );
//        product.SetImageUrls(new List<string> { "https://example.com/image1.jpg" });
//        product.SetVideoUrls(new List<string> { "https://example.com/video1.mp4" });

//        var productRepoMock = new Mock<IProductRepository>();
//        productRepoMock.Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
//            .ReturnsAsync(product);

//        var handler = new GetProductHandler(productRepoMock.Object);
//        var query = new GetProductQuery { Id = productId };

//        // Act
//        var result = await handler.Handle(query, CancellationToken.None);

//        // Assert
//        result.Should().NotBeNull();
//        result.ImageUrls.Should().HaveCount(1);
//        result.VideoUrls.Should().HaveCount(1);
//        result.ImageUrls.Should().Contain("https://example.com/image1.jpg");
//        result.VideoUrls.Should().Contain("https://example.com/video1.mp4");
//    }
//}