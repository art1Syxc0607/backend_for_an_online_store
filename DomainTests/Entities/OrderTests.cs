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
        var order = new Order(user, "ул. Ленина, 1", new List<OrderItemDomainDto>());
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
        var order = new Order(user, "ул. Ленина, 1", new List<OrderItemDomainDto>());
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
        var order = new Order(user, "ул. Ленина, 1", new List<OrderItemDomainDto>());
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
        var order = new Order(user, "ул. Ленина, 1", new List<OrderItemDomainDto>());
        var product = new Product("Phone", 599.99m, 500.99m, 10, "Test product");
        order.AddItem(product, 2);
        order.MarkAsPaid();

        // Act
        Action act = () => order.Cancel();

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*Cannot cancel a paid or shipped order*");
    }
}