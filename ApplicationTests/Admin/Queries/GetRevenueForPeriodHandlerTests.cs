using Application.Commands.Admin.Dashboard;
using Application.Enums;
using Application.Interfaces;
using Application.Queries.Admin.Product;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ApplicationTests.Queries.Admin;

public class GetRevenueForPeriodHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnRevenueAndCost()
    {
        // Arrange
        var orderRepoMock = new Mock<IOrderRepository>();
        orderRepoMock.Setup(x => x.GetRevenueForThePeriodAsync(
            It.IsAny<DateTime>(),
            It.IsAny<DateTime>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Application.DTOs.Admin.Order.RevenueForThePeriodDto
            {
                Revenue = 1500.50m,
                Cost = 1500.50m - 550.20m,
                Income = 550.20m
            });

        orderRepoMock.Setup(x => x.GetCostOfGoodsSoldAsync(
            It.IsAny<DateTime>(),
            It.IsAny<DateTime>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(950.30m);

        var handler = new GetRevenueForThePeriodHandler(orderRepoMock.Object, It.IsAny<ILogger<GetRevenueForThePeriodHandler>>());
        var command = new GetRevenueForThePeriodCommand
        {
            LastDayOfThePriod = DateTime.Today,
            FirstDayOfThePriod = DateTime.Today
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Revenue.Should().Be(1500.50m);
        result.Income.Should().Be(550.20m);
    }

    [Fact]
    public async Task Handle_WhenNoOrders_ShouldReturnZero()
    {
        // Arrange
        var orderRepoMock = new Mock<IOrderRepository>();
        orderRepoMock.Setup(x => x.GetRevenueForThePeriodAsync(
            It.IsAny<DateTime>(),
            It.IsAny<DateTime>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Application.DTOs.Admin.Order.RevenueForThePeriodDto
            {
                Revenue = 0,
                Cost = 0,
                Income = 0
            });

        orderRepoMock.Setup(x => x.GetCostOfGoodsSoldAsync(
            It.IsAny<DateTime>(),
            It.IsAny<DateTime>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(0m);

        var handler = new GetRevenueForThePeriodHandler(orderRepoMock.Object, It.IsAny<ILogger<GetRevenueForThePeriodHandler>>());
        var command = new GetRevenueForThePeriodCommand
        {
            LastDayOfThePriod = DateTime.Today,
            FirstDayOfThePriod = DateTime.Today
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Revenue.Should().Be(0m);
        result.Income.Should().Be(0m);
    }

    //Сводка теста: всего: 2; сбой: 0; успешно: 2; пропущено: 0; длительность: 1,3 с
}