using Application.DTOs.Product;
using Application.Interfaces;
using Application.Queries.Admin.Dashboard;
using Domain.Exceptions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ApplicationTests.Queries.Admin;

public class GetMostPopularProductsHandlerTests
{
    [Fact]
    public async Task Handle_WhenProductsExist_ShouldReturnPopularProducts()
    {
        // Arrange
        var popularProducts = new List<PopularProductDto>
        {
            new() { ProductId = 1, Name = "iPhone", TotalPurchases = 5 },
            new() { ProductId = 2, Name = "AirPods", TotalPurchases = 3 }
        };

        var productRepoMock = new Mock<IProductRepository>();
        productRepoMock.Setup(x => x.GetMostPopularProductsForThePeriodAsync(
            It.IsAny<GetMostPopularProductsForThePeriodCommand>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync((popularProducts, 2));

        var handler = new GetMostPopularProductsForThePeriodHandler(
            productRepoMock.Object,
            Mock.Of<IOrderRepository>(),
            Mock.Of<ILogger<GetMostPopularProductsForThePeriodHandler>>()
        );

        var command = new GetMostPopularProductsForThePeriodCommand
        {
            FirstDayOfThePeriod = DateTime.Today.AddDays(-7),
            LastDayOfThePeriod = DateTime.Today
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(2);
        result.Items[0].Name.Should().Be("iPhone");
        result.Items[0].TotalPurchases.Should().Be(5);
        result.Items[1].Name.Should().Be("AirPods");
        result.Items[1].TotalPurchases.Should().Be(3);
        result.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task Handle_WhenRepositoryReturnsEmpty_ShouldReturnEmptyPage()
    {
        // Arrange
        var productRepoMock = new Mock<IProductRepository>();
        productRepoMock.Setup(x => x.GetMostPopularProductsForThePeriodAsync(
            It.IsAny<GetMostPopularProductsForThePeriodCommand>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<PopularProductDto>(), 0));

        var handler = new GetMostPopularProductsForThePeriodHandler(
            productRepoMock.Object,
            Mock.Of<IOrderRepository>(),
            Mock.Of<ILogger<GetMostPopularProductsForThePeriodHandler>>()
        );

        var command = new GetMostPopularProductsForThePeriodCommand
        {
            FirstDayOfThePeriod = DateTime.Today.AddDays(-7),
            LastDayOfThePeriod = DateTime.Today,
            PageNumber = 2,
            PageSize = 5
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.PageNumber.Should().Be(2);
        result.PageSize.Should().Be(5);
    }

    [Fact]
    public async Task Handle_WhenFirstDayIsAfterLastDay_ShouldThrowDomainException()
    {
        // Arrange
        var handler = new GetMostPopularProductsForThePeriodHandler(
            Mock.Of<IProductRepository>(),
            Mock.Of<IOrderRepository>(),
            Mock.Of<ILogger<GetMostPopularProductsForThePeriodHandler>>()
        );

        var command = new GetMostPopularProductsForThePeriodCommand
        {
            FirstDayOfThePeriod = DateTime.Today,
            LastDayOfThePeriod = DateTime.Today.AddDays(-1)
        };

        // Act
        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*cannot be later*");
    }
}
