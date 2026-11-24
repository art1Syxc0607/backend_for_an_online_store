// WebAPITests/Integration/BusinessRules/OrderBusinessRulesTests.cs
using System.Net;
using WebAPITests.Infrastructure;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Application.DTOs.Order;
using FluentAssertions;

public class OrderBusinessRulesTests : IntegrationTestBase
{
    public OrderBusinessRulesTests(CustomWebApplicationFactory factory) : base(factory) { }

    private async Task<string> InitiatePaymentIntentAsync(int orderId)
    {
        var response = await Client.PostAsJsonAsync("/api/order/initiate", new
        {
            OrderId = orderId,
            Method = "Card"
        });
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<PaymentResult>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.PaymentIntentId.Should().NotBeNullOrEmpty();
        return result.PaymentIntentId!;
    }

    [Fact]
    public async Task PayOrder_WhenAlreadyPaid_ShouldReturn400()
    {
        // Arrange
        var token = await RegisterAndLoginAsync();
        SetAuthToken(token);
        var productId = await CreateProductAsync();
        var orderId = await CreateOrderAsync(productId);

        // Первая оплата
        var paymentIntentId = await InitiatePaymentIntentAsync(orderId);
        var firstConfirm = await Client.PostAsJsonAsync("/api/order/confirm", new { PaymentIntentId = paymentIntentId });
        firstConfirm.StatusCode.Should().Be(HttpStatusCode.OK);

        // Act: вторая оплата (новый intent для уже оплаченного заказа невозможен/должен завершиться ошибкой)
        var secondInitiate = await Client.PostAsJsonAsync("/api/order/initiate", new
        {
            OrderId = orderId,
            Method = "Card"
        });

        // Assert
        secondInitiate.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CancelOrder_WhenPaid_ShouldReturn400()
    {
        // Arrange
        var token = await RegisterAndLoginAsync();
        SetAuthToken(token);
        var productId = await CreateProductAsync();
        var orderId = await CreateOrderAsync(productId);

        var paymentIntentId = await InitiatePaymentIntentAsync(orderId);
        var confirm = await Client.PostAsJsonAsync("/api/order/confirm", new { PaymentIntentId = paymentIntentId });
        confirm.StatusCode.Should().Be(HttpStatusCode.OK);

        // Act
        var response = await Client.PutAsJsonAsync("/api/order/Cancel", new { OrderId = orderId });

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
        using var content = new MultipartFormDataContent
        {
            { new StringContent(productId.ToString()), "ProductId" },
            { new StringContent("Great product review without purchase."), "Text" },
            { new StringContent("5"), "Rating" }
        };
        var response = await Client.PostAsync("/api/review", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}