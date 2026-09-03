using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;
using Domain.DTOs.Order;
using FluentAssertions;
using Xunit;

namespace DomainTests.Entities;

public class OrderTests
{
    [Fact]
    public void AddItem_WhenProductHasEnoughStock_ShouldAddItem()
    {
        // Arrange
        var user = new User("test@mail.com", "hash", "John");
        user.TestsSetUser(1);
        var order = new Order(user.Id, "ул. Ленина, 1", new List<OrderItemDomainDto>());
        var product = new Product("Phone", 599.99m, 500.99m, 10, "Test product");

        // Act
        order.AddItem(product, 2);

        // Assert
        order.Items.Should().HaveCount(1);
        order.TotalAmount.Should().Be(1199.98m);
        order.Status.Should().Be(OrderStatus.Pending);
    }

    [Fact]
    public void AddItem_WhenProductNotEnoughStock_ShouldThrowDomainException()
    {
        // Arrange
        var user = new User("test@mail.com", "hash", "John");
        user.TestsSetUser(1);
        var order = new Order(user.Id, "ул. Ленина, 1", new List<OrderItemDomainDto>());
        var product = new Product("Phone", 599.99m, 500.99m, 10, "Test product");

        // Act
        Action act = () => order.AddItem(product, 50);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*Not enough stock*");
    }

    [Fact]
    public void MarkAsPaid_WhenOrderIsPending_ShouldChangeStatusToPaid()
    {
        // Arrange
        var user = new User("test@mail.com", "hash", "John");
        user.TestsSetUser(1);
        var order = new Order(user.Id, "ул. Ленина, 1", new List<OrderItemDomainDto>());
        var product = new Product("Phone", 599.99m, 500.99m, 10, "Test product");
        order.AddItem(product, 2);

        // Act
        order.MarkAsPaid();

        // Assert
        order.Status.Should().Be(OrderStatus.Paid);
        order.PaidAt.Should().NotBeNull();
    }

    [Fact]
    public void Cancel_WhenOrderIsPaid_ShouldThrowDomainException()
    {
        // Arrange
        var user = new User("test@mail.com", "hash", "John");
        user.TestsSetUser(1);
        var order = new Order(user.Id, "ул. Ленина, 1", new List<OrderItemDomainDto>());
        var product = new Product("Phone", 599.99m, 500.99m, 10, "Test product");
        order.AddItem(product, 2);
        order.MarkAsPaid();

        // Act
        Action act = () => order.Cancel();

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*Cannot cancel a paid or shipped order*");
    }

    // после добавление полной статистики по каждому товару
    [Fact]
    public void MarkAsPaid_ShouldIncreaseAmountOfPaid()
    {
        // Arrange
        var product = new Product("Phone", 599.99m, 500.99m, 10, "Test product");
        var user = new User("test@mail.com", "hash", "John");
        user.TestsSetUser(1);
        var order = new Order(user.Id, "ул. Ленина, 1", new List<OrderItemDomainDto>());
        order.AddItem(product, 2);

        // Act
        order.MarkAsPaid();

        // Assert
        product.AmountOfPaid.Should().Be(2);
        product.AmountOfReceived.Should().Be(0);
        product.AmountOfCanceled.Should().Be(0);
    }

    [Fact]
    public void ReceivedByUser_ShouldIncreaseAmountOfReceived()
    {
        // Arrange
        var product = new Product("Phone", 599.99m, 500.99m, 10, "Test product");
        var user = new User("test@mail.com", "hash", "John");
        user.TestsSetUser(1);
        var order = new Order(user.Id, "ул. Ленина, 1", new List<OrderItemDomainDto>());
        order.AddItem(product, 2);
        order.MarkAsPaid();
        order.Ship();
        order.Deliver();

        // Act
        order.ReceivedByUser();

        // Assert
        product.AmountOfReceived.Should().Be(2);
    }

    [Fact]
    public void Cancel_ShouldIncreaseAmountOfCanceled()
    {
        // Arrange
        var product = new Product("Phone", 599.99m, 500.99m, 10, "Test product");
        var user = new User("test@mail.com", "hash", "John");
        user.TestsSetUser(1);
        var order = new Order(user.Id, "ул. Ленина, 1", new List<OrderItemDomainDto>());
        order.AddItem(product, 2);

        // Act
        order.Cancel();

        // Assert
        product.AmountOfCanceled.Should().Be(2);
        product.AmountOfPaid.Should().Be(0);
    }

    // All tests complets succesfull so far
}