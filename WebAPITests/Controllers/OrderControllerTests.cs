// WebAPITests/Controllers/OrderControllerTests.cs
using Application.Commands.User;
using Application.DTOs.Order;
using Application.DTOs.User;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using WebAPITests.Common;
using Xunit;

namespace WebAPITests.Controllers;

public class OrderControllerTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;

    public OrderControllerTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetOrderHistory_WhenUnauthorized_ShouldReturn401()
    {
        // Act
        var response = await _client.GetAsync("/api/order/history");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetOrderHistory_WhenAuthorized_ShouldReturn200()
    {
        // Arrange
        var token = await GetAuthToken();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/api/order/history");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CreateOrder_WhenAuthorized_ShouldReturn200()
    {
        // Arrange
        var token = await GetAuthToken();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var dto = new CreateOrderDto
        {
            Items = new List<OrderItemDto>
            {
                new() { ProductId = 1, Quantity = 2, PriceAtPurchase = 999.99m }
            },
            ShippingAddress = "ул. Ленина, 1"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/order", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CreateOrder_WhenUnauthorized_ShouldReturn401()
    {
        // Arrange
        var dto = new CreateOrderDto
        {
            Items = new List<OrderItemDto>(),
            ShippingAddress = "ул. Ленина, 1"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/order", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private async Task<string> GetAuthToken()
    {
        // Регистрируем и логиним пользователя
        var registerCommand = new RegisterCommand
        {
            Email = "orderuser@mail.com",
            UserName = "OrderUser",
            Password = "Password123!"
        };
        await _client.PostAsJsonAsync("/api/auth/register", registerCommand);

        var loginDto = new LoginDto
        {
            Email = "orderuser@mail.com",
            Password = "Password123!"
        };
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginDto);
        var result = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        return result!.Token;
    }
}