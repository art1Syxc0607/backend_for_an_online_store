// WebAPITests/Controllers/ProductControllerTests.cs
using System.Net;
using System.Net.Http.Json;
using Application.DTOs.Product;
using FluentAssertions;
using WebAPITests.Common;
using Xunit;

namespace WebAPITests.Controllers;

public class ProductControllerTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ProductControllerTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAllProducts_ShouldReturn200()
    {
        // Act
        var response = await _client.GetAsync("/api/product");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var products = await response.Content.ReadFromJsonAsync<List<ProductResponseDto>>();
        products.Should().NotBeNull();
    }

    [Fact]
    public async Task GetProductsFilter_ShouldReturn200()
    {
        // Act
        var response = await _client.GetAsync("/api/product?page=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetProductsFilter_WithSearch_ShouldReturn200()
    {
        // Act
        var response = await _client.GetAsync("/api/product?search=iPhone");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetProductsFilter_WithCategory_ShouldReturn200()
    {
        // Act
        var response = await _client.GetAsync("/api/product?categoryId=1");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetProductsFilter_WithPriceRange_ShouldReturn200()
    {
        // Act
        var response = await _client.GetAsync("/api/product?minPrice=100&maxPrice=1000");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}