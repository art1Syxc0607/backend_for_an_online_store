// WebAPITests/Controllers/ReviewControllerTests.cs
using Application.Commands.User;
using Application.DTOs.Review;
using Application.DTOs.User;
using Domain.Enums;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using WebApi.DTOs.Review;
using WebAPITests.Common;
using Xunit;

namespace WebAPITests.Controllers;

public class ReviewControllerTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ReviewControllerTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetProductReviews_ShouldReturn200()
    {
        // Act
        var response = await _client.GetAsync("/api/review/1");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var reviews = await response.Content.ReadFromJsonAsync<List<ReviewResponseDto>>();
        reviews.Should().NotBeNull();
    }

    [Fact]
    public async Task LeaveComment_WhenUnauthorized_ShouldReturn401()
    {
        // Arrange
        var dto = new AddReviewDto
        {
            ProductId = 1,
            Text = "Great product!",
            Rating = 5
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/review", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task LeaveComment_WhenAuthorized_ShouldReturn200()
    {
        // Arrange
        var token = await GetAuthToken();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var dto = new AddReviewDto
        {
            ProductId = 1,
            Text = "Great product!",
            Rating = 5
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/review", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    private async Task<string> GetAuthToken()
    {
        var registerCommand = new RegisterCommand
        {
            Email = "reviewuser@mail.com",
            UserName = "ReviewUser",
            Password = "Password123!"
        };
        await _client.PostAsJsonAsync("/api/auth/register", registerCommand);

        var loginDto = new LoginDto
        {
            Email = "reviewuser@mail.com",
            Password = "Password123!"
        };
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginDto);
        var result = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        return result!.Token;
    }
}