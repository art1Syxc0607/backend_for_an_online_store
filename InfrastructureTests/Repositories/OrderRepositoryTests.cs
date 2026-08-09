// InfrastructureTests/Repositories/OrderRepositoryTests.cs
using Application.DTOs.Order;
using Domain.DTOs.Order;
using Domain.Entities;
using Domain.Enums;
using FluentAssertions;
using Infrastructure.Repositories;
using InfrastructureTests.Common;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace InfrastructureTests.Repositories;

public class OrderRepositoryTests : IClassFixture<TestDatabaseFixture>
{
    private readonly TestDatabaseFixture _fixture;
    private readonly OrderRepository _repository;

    public OrderRepositoryTests(TestDatabaseFixture fixture)
    {
        _fixture = fixture;
        _repository = new OrderRepository(_fixture.Context);
    }

    [Fact]
    public async Task CreateOrder_ShouldAddOrder()
    {
        // Arrange
        var user = new User("test@mail.com", "hash", "John");
        await _fixture.Context.Users.AddAsync(user);
        await _fixture.Context.SaveChangesAsync();

        var order = new Order(user, "ул. Ленина, 1", new List<OrderItemDomainDto>());

        // Act
        await _repository.CreateOrder(order);
        await _fixture.Context.SaveChangesAsync();

        // Assert
        var saved = await _fixture.Context.Orders.Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.UserId == user.Id); // ← ищем по ID пользователя!
        saved.Should().NotBeNull();
        saved!.UserId.Should().Be(user.Id);
        saved!.Status.Should().Be(OrderStatus.Pending);
    }

    [Fact]
    public async Task GetOrder_WhenOrderExists_ShouldReturnOrder()
    {
        // Arrange
        var user = new User("test@mail.com", "hash", "John");
        await _fixture.Context.Users.AddAsync(user);
        await _fixture.Context.SaveChangesAsync();

        var order = new Order(user, "ул. Ленина, 1", new List<OrderItemDomainDto>());
        await _repository.CreateOrder(order);
        await _fixture.Context.SaveChangesAsync();

        // Act
        var result = await _repository.GetOrder(order.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(order.Id);
        result!.UserId.Should().Be(user.Id);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnUserOrders()
    {
        // Arrange
        var user = new User("test@mail.com", "hash", "John");
        await _fixture.Context.Users.AddAsync(user);
        await _fixture.Context.SaveChangesAsync();

        var order1 = new Order(user, "ул. Ленина, 1", new List<OrderItemDomainDto>());
        var order2 = new Order(user, "ул. Пушкина, 2", new List<OrderItemDomainDto>());

        await _repository.CreateOrder(order1);
        await _repository.CreateOrder(order2);
        await _fixture.Context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync(user.Id);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(o => o.ShippingAddress == "ул. Ленина, 1");
        result.Should().Contain(o => o.ShippingAddress == "ул. Пушкина, 2");
    }

    [Fact]
    public async Task HasUserPurchasedProductAsync_WhenPurchased_ShouldReturnTrue()
    {
        // Arrange
        var user = new User("test@mail.com", "hash", "John");
        var product = new Product("iPhone", 999.99m, 900.99m, 10, "Latest iPhone");


        await _fixture.Context.Users.AddAsync(user);
        await _fixture.Context.Products.AddAsync(product);
        await _fixture.Context.SaveChangesAsync();

        var order = new Order(user, "ул. Ленина, 1", new List<OrderItemDomainDto>());
        order.AddItem(product, 1);
        order.MarkAsPaid();
        order.Ship();
        order.Deliver();
        order.ReceivedByUser(); // ← добавляем подтверждение получения!

        await _repository.CreateOrder(order);
        await _fixture.Context.SaveChangesAsync();

        // Act
        var result = await _repository.HasUserPurchasedProductAsync(user.Id, product.Id);

        // Assert
        result.Should().BeTrue();
    }
}