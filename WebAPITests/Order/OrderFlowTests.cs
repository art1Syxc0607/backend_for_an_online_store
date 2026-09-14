// WebAPITests/Integration/Order/OrderFlowTests.cs
using System.Net;
using System.Net.Http.Json;
using Application.DTOs.Order;
using FluentAssertions;
using WebAPITests.Infrastructure;

namespace WebAPITests.Integration.Order;

public class OrderFlowTests : IntegrationTestBase
{
    public OrderFlowTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task CreateOrder_WhenValid_ShouldReturnOrderId()
    {
        // Arrange
        var token = await RegisterAndLoginAsync();
        SetAuthToken(token);

        var productId = await CreateProductAsync("iPhone", 999.99m, 10);

        // Act
        var orderId = await CreateOrderAsync(productId, 2);

        // Assert
        orderId.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task CreateOrder_WhenNotEnoughStock_ShouldReturn400()
    {
        // Arrange
        var token = await RegisterAndLoginAsync();
        SetAuthToken(token);

        var productId = await CreateProductAsync("Limited", 100m, 1);

        // Act
        var response = await Client.PostAsJsonAsync("/api/order", new
        {
            ShippingAddress = "ул. Ленина, д. 1",
            Items = new[] { new { ProductId = productId, Quantity = 5 } }
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task FullOrderFlow_FromCreateToReceive_ShouldSucceed()
    {
        // Arrange
        var userToken = await RegisterAndLoginAsync("buyer@mail.com");
        var adminToken = await LoginAsAdminAsync();

        SetAuthToken(adminToken); // нужно для админских операций
        var productId = await CreateProductAsync("MacBook", 1999m, 5);

        // 1. Создание заказа
        SetAuthToken(userToken);
        var orderId = await CreateOrderAsync(productId, 1);

        // 2. Оплата (эмуляция webhook)
        var paymentResponse = await Client.PostAsJsonAsync($"/api/payments/{orderId}/confirm", new { });
        paymentResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // 3. Отгрузка (админ)
        SetAuthToken(adminToken);
        var shipResponse = await Client.PostAsync($"/api/admin/orders/{orderId}/ship", null);
        shipResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // 4. Доставка (админ)
        var deliverResponse = await Client.PostAsync($"/api/admin/orders/{orderId}/deliver", null);
        deliverResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // 5. Получение (пользователь)
        SetAuthToken(userToken);
        var receiveResponse = await Client.PostAsync($"/api/order/{orderId}/receive", null);
        receiveResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Assert: проверяем финальный статус
        var orderResponse = await Client.GetAsync($"/api/order/{orderId}");
        var order = await orderResponse.Content.ReadFromJsonAsync<OrderResponseDto>();
        order!.Status.Should().Be(Domain.Enums.OrderStatus.Received);
    }

    [Fact]
    public async Task GetOrder_WhenAnotherUser_ShouldReturn403()
    {
        // Arrange
        var user1Token = await RegisterAndLoginAsync("user1@mail.com");
        var user2Token = await RegisterAndLoginAsync("user2@mail.com");

        SetAuthToken(user1Token);
        var productId = await CreateProductAsync();
        var orderId = await CreateOrderAsync(productId);

        // Act: user2 пытается получить заказ user1
        SetAuthToken(user2Token);
        var response = await Client.GetAsync($"/api/order/{orderId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CancelOrder_WhenPending_ShouldReturn200()
    {
        // Arrange
        var token = await RegisterAndLoginAsync();
        SetAuthToken(token);
        var productId = await CreateProductAsync();
        var orderId = await CreateOrderAsync(productId);

        // Act
        var response = await Client.PostAsync($"/api/order/{orderId}/cancel", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}