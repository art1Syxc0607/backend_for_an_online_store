// ApplicationTests/Queries/Order/GetOrderHistoryHandlerTests.cs
using Application.DTOs.Order;
using Application.Interfaces;
using Application.Queries.Order;
using Domain.Entities;
using Domain.DTOs.Order;
using Domain.Enums;
using FluentAssertions;
using Moq;
using Xunit;

namespace ApplicationTests.Queries.Order;

public class GetOrderHistoryHandlerTests
{
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly GetOrderHistoryQueryHandler _handler;

    public GetOrderHistoryHandlerTests()
    {
        _orderRepositoryMock = new Mock<IOrderRepository>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _handler = new GetOrderHistoryQueryHandler(_orderRepositoryMock.Object, _userRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WhenUserExistsAndHasOrders_ShouldReturnOrderHistory()
    {
        // Arrange
        var userId = 1;
        var user = new User("test@mail.com", "hash", "John");

        var orders = new List<Domain.Entities.Order>
        {
            CreateOrderWithId(user, 1, 150.50m, OrderStatus.Paid, DateTime.UtcNow.AddDays(-5)),
            CreateOrderWithId(user, 2, 250.00m, OrderStatus.Delivered, DateTime.UtcNow.AddDays(-10)),
            CreateOrderWithId(user, 3, 100.00m, OrderStatus.Pending, DateTime.UtcNow.AddDays(-1))
        };

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _orderRepositoryMock
            .Setup(x => x.GetAllAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(orders);

        var query = new GetOrderHistoryQuery { userId = userId };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        result.Should().BeInDescendingOrder(o => o.CreatedAt);

        result[0].Id.Should().Be(3);
        result[0].TotalAmount.Should().Be(100.00m);
        result[0].Status.Should().Be(OrderStatus.Pending);

        result[1].Id.Should().Be(1);
        result[1].TotalAmount.Should().Be(150.50m);
        result[1].Status.Should().Be(OrderStatus.Paid);

        result[2].Id.Should().Be(2);
        result[2].TotalAmount.Should().Be(250.00m);
        result[2].Status.Should().Be(OrderStatus.Delivered);

        _orderRepositoryMock.Verify(x => x.GetAllAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserExistsButHasNoOrders_ShouldReturnEmptyList()
    {
        // Arrange
        var userId = 1;
        var user = new User("test@mail.com", "hash", "John");

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _orderRepositoryMock
            .Setup(x => x.GetAllAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Domain.Entities.Order>());

        var query = new GetOrderHistoryQuery { userId = userId };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenUserDoesNotExist_ShouldThrowDomainException()
    {
        // Arrange
        var userId = 999;

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var query = new GetOrderHistoryQuery { userId = userId };

        // Act
        Func<Task> act = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Domain.Exceptions.DomainException>()
            .WithMessage("*User not found*");

        _orderRepositoryMock.Verify(
            x => x.GetAllAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Fact]
    public async Task Handle_WhenOrderContainsItems_ShouldMapItemsCorrectly()
    {
        // Arrange
        var userId = 1;
        var user = new User("test@mail.com", "hash", "John");

        var product = new Domain.Entities.Product("iPhone", 999.99m, 900.99m, 10, "Test product", null);
        product.TestsSetProduct(1);

        var order = CreateOrderWithItems(user, 1, product, 2, 999.99m);

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _orderRepositoryMock
            .Setup(x => x.GetAllAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Domain.Entities.Order> { order });

        var query = new GetOrderHistoryQuery { userId = userId };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        result[0].Items.Should().HaveCount(1);
        result[0].Items[0].ProductNameAtPurchase.Should().Be("iPhone");
        result[0].Items[0].Quantity.Should().Be(2);
        result[0].Items[0].PriceAtPurchase.Should().Be(999.99m);
    }

    [Fact]
    public async Task Handle_WhenMultipleOrders_ShouldOrderByCreatedAtDescending()
    {
        // Arrange
        var userId = 1;
        var user = new User("test@mail.com", "hash", "John");

        var now = DateTime.UtcNow;
        var orders = new List<Domain.Entities.Order>
        {
            CreateOrderWithId(user, 1, 100m, OrderStatus.Pending, now.AddHours(-1)),
            CreateOrderWithId(user, 2, 200m, OrderStatus.Paid, now.AddHours(-3)),
            CreateOrderWithId(user, 3, 300m, OrderStatus.Delivered, now.AddHours(-2))
        };

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _orderRepositoryMock
            .Setup(x => x.GetAllAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(orders);

        var query = new GetOrderHistoryQuery { userId = userId };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeInDescendingOrder(o => o.CreatedAt);
        result[0].Id.Should().Be(1); // самый новый
        result[1].Id.Should().Be(3); // средний
        result[2].Id.Should().Be(2); // самый старый
    }

    [Fact]
    public async Task Handle_WhenOrderHasMultipleItems_ShouldMapAllItems()
    {
        // Arrange
        var userId = 1;
        var user = new User("test@mail.com", "hash", "John");

        var product1 = new Domain.Entities.Product("iPhone", 999.99m, 900.99m, 10, "Smartphone", null);
        product1.TestsSetProduct(1);

        var product2 = new Domain.Entities.Product("AirPods", 199.99m, 900.99m, 20, "Wireless headphones", null);
        product2.TestsSetProduct(2);

        var order = new Domain.Entities.Order(user, "ул. Ленина, 1", new List<OrderItemDomainDto>());
        order.TestsSetOrder(1);
        order.AddItem(product1, 1);
        order.AddItem(product2, 2);

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _orderRepositoryMock
            .Setup(x => x.GetAllAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Domain.Entities.Order> { order });

        var query = new GetOrderHistoryQuery { userId = userId };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result[0].Items.Should().HaveCount(2);
        result[0].Items.Should().Contain(i => i.ProductNameAtPurchase == "iPhone");
        result[0].Items.Should().Contain(i => i.ProductNameAtPurchase == "AirPods");
        result[0].TotalAmount.Should().Be(1399.97m); // 999.99 + 199.99*2
    }

    #region Helper Methods

    private Domain.Entities.Order CreateOrderWithId(User user, int id, decimal totalAmount, OrderStatus status, DateTime createdAt)
    {
        var order = new Domain.Entities.Order(user, "ул. Ленина, 1", new List<OrderItemDomainDto>());


        // Используем рефлексию для установки полей (только для тестов)
        //typeof(Domain.Entities.Order).GetField("_totalAmount", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
        //    ?.SetValue(order, totalAmount);
        //typeof(Domain.Entities.Order).GetField("_status", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
        //    ?.SetValue(order, status);
        //typeof(Domain.Entities.Order).GetField("_createdAt", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
        //    ?.SetValue(order, createdAt);

        order.TestsSetOrder(id, totalAmount, createdAt, status);

        return order;
    }

    private Domain.Entities.Order CreateOrderWithItems(User user, int id, Domain.Entities.Product product, 
        int quantity, decimal price)
    {
        var order = new Domain.Entities.Order(user, "ул. Ленина, 1", new List<OrderItemDomainDto>());
        order.TestsSetOrder(id);
        order.AddItem(product, quantity);

        return order;
    }

    #endregion
}