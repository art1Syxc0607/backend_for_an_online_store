// InfrastructureTests/Services/PaymentServiceTests.cs
using Domain.Enums;
using FluentAssertions;
using Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace InfrastructureTests.Services;

public class PaymentServiceTests
{
    private readonly PaymentService _paymentService;
    //private readonly IConfiguration _configuration;

    public PaymentServiceTests(/*IConfiguration configuration*/)
    {
        //_configuration = configuration;
        _paymentService = new PaymentService(/*_configuration, */NullLogger<PaymentService>.Instance);
    }

    [Fact]
    public async Task InitiatePaymentAsync_ShouldReturnPaymentResult()
    {
        // Arrange
        var orderId = 1;
        var method = PaymentMethod.Card;

        // Act
        var result = await _paymentService.InitiatePaymentAsync(orderId, method);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.PaymentIntentId.Should().StartWith($"pi_{orderId}_");
        result.ClientSecret.Should().NotBeNullOrEmpty();
        result.RedirectUrl.Should().Contain($"pay/{orderId}");
    }

    [Fact]
    public async Task ConfirmPaymentAsync_ShouldReturnConfirmation()
    {
        // Arrange
        var paymentIntentId = "pi_123";

        // Act
        var result = await _paymentService.ConfirmPaymentAsync(paymentIntentId);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.TransactionId.Should().Be(paymentIntentId);
    }

    [Fact]
    public async Task RefundPaymentAsync_ShouldReturnRefundResult()
    {
        // Arrange
        var orderId = 1;
        var amount = 100m;

        // Act
        var result = await _paymentService.RefundPaymentAsync(orderId, amount);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.RefundId.Should().StartWith($"ref_{orderId}_");
    }
}