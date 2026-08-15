// WebAPITests/Controllers/CartControllerTests.cs
using Application.Commands.User;
using Application.DTOs.Cart;
using Application.DTOs.User;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using WebAPITests.Common;
using Xunit;

namespace WebAPITests.Controllers;

public class CartControllerTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;

    public CartControllerTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetCart_WhenUnauthorized_ShouldReturn401()
    {
        // Act
        var response = await _client.GetAsync("/api/cart");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetCart_WhenAuthorized_ShouldReturn200()
    {
        // Arrange
        var token = await GetAuthToken();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/api/cart");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var cart = await response.Content.ReadFromJsonAsync<CartResponseDto>();
        cart.Should().NotBeNull();
    }

    [Fact]
    public async Task AddToCart_WhenAuthorized_ShouldReturn204()
    {
        // Arrange
        var token = await GetAuthToken();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var dto = new AddCartItemDto
        {
            ProductId = 1,
            Quantity = 2
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/cart", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    private async Task<string> GetAuthToken()
    {
        var registerCommand = new RegisterCommand
        {
            Email = "cartuser@mail.com",
            UserName = "CartUser",
            Password = "Password123!"
        };
        await _client.PostAsJsonAsync("/api/auth/register", registerCommand);

        var loginDto = new LoginDto
        {
            Email = "cartuser@mail.com",
            Password = "Password123!"
        };
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginDto);
        var result = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        return result!.Token;
    }
}