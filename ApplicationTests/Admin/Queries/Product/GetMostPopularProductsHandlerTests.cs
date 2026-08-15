using Application.Commands.Admin.Dashboard;
using Application.DTOs.Product;
using Application.Enums;
using Application.Interfaces;
using Application.Queries.Admin.Product;
using Domain.Entities;
using Domain.Enums;
using FluentAssertions;
using Moq;
using Xunit;

namespace ApplicationTests.Queries.Admin;

public class GetMostPopularProductsHandlerTests
{
    [Fact]
    public async Task Handle_WhenProductsExist_ShouldReturnPopularProducts()
    {
        // Arrange
        var product1 = new Domain.Entities.Product("iPhone", 999.99m, 750m, 10, "Test");
        //product1.AmountOfPaid = 5;
        //product1.AmountOfReceived = 5;
        product1.TestsSetProduct(1, 5, 5);

        var product2 = new Domain.Entities.Product("AirPods", 199.99m, 140m, 20, "Test");
        //product2.AmountOfPaid = 3;
        //product2.AmountOfReceived = 3;
        product2.TestsSetProduct(1, 3, 3);

        var popularProducts = new List<PopularProductDto>
        {
            new() { ProductId = 1, Name = "iPhone", TotalPurchases = 5 },
            new() { ProductId = 2, Name = "AirPods", TotalPurchases = 3 }
        };

        var productRepoMock = new Mock<IProductRepository>();
        productRepoMock.Setup(x => x.GetMostPopularProductsForThePeriod(
            It.IsAny<DateSpan>(),
            It.IsAny<DateTime>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(popularProducts);

        var cacheServiceMock = new Mock<ICacheService>();
        cacheServiceMock.Setup(x => x.GetAsync<List<PopularProductDto>>(It.IsAny<string>()))
            .ReturnsAsync((List<PopularProductDto>?)null);

        var handler = new GetMostPopularProductsForThePeriodHandler(
            productRepoMock.Object,
            Mock.Of<IOrderRepository>(),
            cacheServiceMock.Object
        );

        var command = new GetMostPopularProductsForThePeriodCommand
        {
            Span = DateSpan.Week,
            LastDayOfThePriod = DateTime.Today
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result[0].Name.Should().Be("iPhone");
        result[0].TotalPurchases.Should().Be(5);
        result[1].Name.Should().Be("AirPods");
        result[1].TotalPurchases.Should().Be(3);
        cacheServiceMock.Verify(x => x.SetAsync(
            It.IsAny<string>(),
            It.IsAny<List<PopularProductDto>>(),
            It.IsAny<TimeSpan?>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenCachedDataExists_ShouldReturnFromCache()
    {
        // Arrange
        var cachedData = new List<PopularProductDto>
        {
            new() { ProductId = 1, Name = "iPhone", TotalPurchases = 10 }
        };

        var cacheServiceMock = new Mock<ICacheService>();
        cacheServiceMock.Setup(x => x.GetAsync<List<PopularProductDto>>(It.IsAny<string>()))
            .ReturnsAsync(cachedData);

        var handler = new GetMostPopularProductsForThePeriodHandler(
            Mock.Of<IProductRepository>(),
            Mock.Of<IOrderRepository>(),
            cacheServiceMock.Object
        );

        var command = new GetMostPopularProductsForThePeriodCommand
        {
            Span = DateSpan.Week,
            LastDayOfThePriod = DateTime.Today
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        result[0].Name.Should().Be("iPhone");
        cacheServiceMock.Verify(x => x.SetAsync(
            It.IsAny<string>(),
            It.IsAny<List<PopularProductDto>>(),
            It.IsAny<TimeSpan?>()), Times.Never);
    }

    //done 
    //Сводка теста: всего: 2; сбой: 0; успешно: 2; пропущено: 0; длительность: 1,9 с
}