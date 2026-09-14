// WebAPITests/Integration/BusinessRules/OrderBusinessRulesTests.cs
using System.Net;
using WebAPITests.Infrastructure;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;

public class OrderBusinessRulesTests : IntegrationTestBase
{
    public OrderBusinessRulesTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task PayOrder_WhenAlreadyPaid_ShouldReturn400()
    {
        // Arrange
        var token = await RegisterAndLoginAsync();
        SetAuthToken(token);
        var productId = await CreateProductAsync();
        var orderId = await CreateOrderAsync(productId);

        // Первая оплата
        await Client.PostAsJsonAsync($"/api/payments/{orderId}/confirm", new { });

        // Act: вторая оплата
        var response = await Client.PostAsJsonAsync($"/api/payments/{orderId}/confirm", new { });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CancelOrder_WhenPaid_ShouldReturn400()
    {
        // Arrange
        var token = await RegisterAndLoginAsync();
        SetAuthToken(token);
        var productId = await CreateProductAsync();
        var orderId = await CreateOrderAsync(productId);

        await Client.PostAsJsonAsync($"/api/payments/{orderId}/confirm", new { });

        // Act
        var response = await Client.PostAsync($"/api/order/{orderId}/cancel", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateReview_WhenNotPurchased_ShouldReturn400()
    {
        // Arrange
        var token = await RegisterAndLoginAsync();
        SetAuthToken(token);
        var productId = await CreateProductAsync();

        // Act: отзыв без покупки
        var response = await Client.PostAsJsonAsync($"/api/products/{productId}/reviews", new
        {
            Text = "Great product!",
            Rating = 5
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}