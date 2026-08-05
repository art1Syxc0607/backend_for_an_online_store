// InfrastructureTests/Repositories/CartRepositoryTests.cs
using Domain.Entities;
using FluentAssertions;
using Infrastructure.Repositories;
using InfrastructureTests.Common;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace InfrastructureTests.Repositories;

public class CartRepositoryTests : IClassFixture<TestDatabaseFixture>
{
    private readonly TestDatabaseFixture _fixture;
    private readonly CartRepository _repository;

    public CartRepositoryTests(TestDatabaseFixture fixture)
    {
        _fixture = fixture;
        _repository = new CartRepository(_fixture.Context);
    }

    [Fact]
    public async Task GetByUserIdAsync_WhenCartExists_ShouldReturnCart()
    {
        // Arrange
        var user = new User("test@mail.com", "hash", "John");
        await _fixture.Context.Users.AddAsync(user);
        await _fixture.Context.SaveChangesAsync();

        var cart = new Cart(user);
        await _fixture.Context.Carts.AddAsync(cart);
        await _fixture.Context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByUserIdAsync(user.Id);

        // Assert
        result.Should().NotBeNull();
        result!.UserId.Should().Be(user.Id);
    }

    [Fact]
    public async Task GetByUserIdAsync_WhenCartHasItems_ShouldIncludeItems()
    {
        // Arrange
        var user = new User("test@mail.com", "hash", "John");
        var product = new Product("iPhone", 999.99m, 10, "Latest iPhone");

        await _fixture.Context.Users.AddAsync(user);
        await _fixture.Context.Products.AddAsync(product);
        await _fixture.Context.SaveChangesAsync();

        var cart = new Cart(user);
        cart.AddItem(product, 2);

        await _fixture.Context.Carts.AddAsync(cart);
        await _fixture.Context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByUserIdAsync(user.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Items.Should().HaveCount(1);
        result!.Items.First().ProductId.Should().Be(product.Id);
        result!.Items.First().Quantity.Should().Be(2);
    }
}