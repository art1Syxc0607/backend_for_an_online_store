// WebAPITests/Controllers/CategoryControllerTests.cs
using System.Net;
using System.Net.Http.Json;
using Application.DTOs.Category;
using FluentAssertions;
using WebAPITests.Common;
using Xunit;

namespace WebAPITests.Controllers;

public class CategoryControllerTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;

    public CategoryControllerTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAllCategories_ShouldReturn200()
    {
        // Act
        var response = await _client.GetAsync("/api/category");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var categories = await response.Content.ReadFromJsonAsync<List<CategoryResponseDto>>();
        categories.Should().NotBeNull();
    }

    [Fact]
    public async Task GetCategoryById_WhenExists_ShouldReturn200()
    {
        // Act
        var response = await _client.GetAsync("/api/category/1");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var category = await response.Content.ReadFromJsonAsync<CategoryResponseDto>();
        category.Should().NotBeNull();
    }

    [Fact]
    public async Task GetCategoryById_WhenNotExists_ShouldReturn404()
    {
        // Act
        var response = await _client.GetAsync("/api/category/999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}