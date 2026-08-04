// InfrastructureTests/Repositories/ProductRepositoryTests.cs
using Domain.Entities;
using FluentAssertions;
using Infrastructure.Repositories;
using InfrastructureTests.Common;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace InfrastructureTests.Repositories;

public class ProductRepositoryTests : IClassFixture<TestDatabaseFixture>
{
    private readonly TestDatabaseFixture _fixture;
    private readonly ProductRepository _repository;

    public ProductRepositoryTests(TestDatabaseFixture fixture)
    {
        _fixture = fixture;
        _repository = new ProductRepository(_fixture.Context);
    }

    [Fact]
    public async Task AddProductAsync_ShouldAddProduct()
    {
        // Arrange
        var product = new Product("iPhone", 999.99m, 10, "Latest iPhone");

        // Act
        await _repository.AddProductAsync(product);
        await _fixture.Context.SaveChangesAsync();

        // Assert
        var saved = await _fixture.Context.Products.FirstOrDefaultAsync(p => p.Name == "iPhone");
        saved.Should().NotBeNull();
        saved!.Price.Should().Be(999.99m);
    }

    [Fact]
    public async Task GetByIdAsync_WhenProductExists_ShouldReturnProduct()
    {
        // Arrange
        var product = new Product("iPhone", 999.99m, 10, "Latest iPhone");
        await _repository.AddProductAsync(product);
        await _fixture.Context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(product.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("iPhone");
    }

    [Fact]
    public async Task GetByIdsAsync_WhenProductsExist_ShouldReturnAllProducts()
    {
        // Arrange
        var products = new List<Product>
        {
            new Product("iPhone", 999.99m, 10, "Latest iPhone"),
            new Product("AirPods", 199.99m, 20, "Wireless headphones")
        };

        foreach (var p in products)
            await _repository.AddProductAsync(p);

        await _fixture.Context.SaveChangesAsync();

        var ids = products.Select(p => p.Id).ToList();

        // Act
        var result = await _repository.GetByIdsAsync(ids);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(p => p.Name == "iPhone");
        result.Should().Contain(p => p.Name == "AirPods");
    }

    [Fact]
    public async Task UpdateProductAsync_ShouldUpdateProduct()
    {
        // Arrange
        var product = new Product("iPhone", 999.99m, 10, "Latest iPhone");
        await _repository.AddProductAsync(product);
        await _fixture.Context.SaveChangesAsync();

        // Act
        product.UpdateDetails(name: "iPhone Pro", price: 1299.99m);
        await _repository.UpdateProductAsync(product);
        await _fixture.Context.SaveChangesAsync();

        // Assert
        var updated = await _fixture.Context.Products.FirstOrDefaultAsync(p => p.Id == product.Id);
        updated!.Name.Should().Be("iPhone Pro");
        updated!.Price.Should().Be(1299.99m);
    }

    [Fact]
    public async Task DeleteProductAsync_ShouldDeleteProduct()
    {
        // Arrange
        var product = new Product("iPhone", 999.99m, 10, "Latest iPhone");
        await _repository.AddProductAsync(product);
        await _fixture.Context.SaveChangesAsync();

        // Act
        await _repository.DeleteProductAsync(product);
        await _fixture.Context.SaveChangesAsync();

        // Assert
        var deleted = await _fixture.Context.Products.FirstOrDefaultAsync(p => p.Id == product.Id);
        deleted.Should().BeNull();
    }
}