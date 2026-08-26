// ApplicationTests/Services/PaymentServiceTests.cs
using Application.DTOs.Order;
using Application.Enums;
using Application.Interfaces;
using Domain.Enums;
using FluentAssertions;
using Infrastructure.Services;
using Infrastructure.Services.Payment;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace ApplicationTests.Services;

public class PaymentServiceTests
{
    private readonly Mock<IPaymentStrategy> _cardStrategyMock;
    private readonly Mock<IPaymentStrategy> _googlePayStrategyMock;
    private readonly Mock<IPaymentStrategy> _applePayStrategyMock;
    private readonly Mock<IPaymentStrategy> _sbpStrategyMock;
    private readonly IPaymentStrategyFactory _factory;
    private readonly PaymentService _paymentService;

    public PaymentServiceTests()
    {
        _cardStrategyMock = new Mock<IPaymentStrategy>();
        _cardStrategyMock.Setup(x => x.Method).Returns(PaymentMethod.Card);

        _googlePayStrategyMock = new Mock<IPaymentStrategy>();
        _googlePayStrategyMock.Setup(x => x.Method).Returns(PaymentMethod.GooglePay);

        _applePayStrategyMock = new Mock<IPaymentStrategy>();
        _applePayStrategyMock.Setup(x => x.Method).Returns(PaymentMethod.ApplePay);

        _sbpStrategyMock = new Mock<IPaymentStrategy>();
        _sbpStrategyMock.Setup(x => x.Method).Returns(PaymentMethod.SBP);

        var strategies = new List<IPaymentStrategy>
        {
            _cardStrategyMock.Object,
            _googlePayStrategyMock.Object,
            _applePayStrategyMock.Object,
            _sbpStrategyMock.Object
        };

        _factory = new PaymentStrategyFactory(strategies);
        _paymentService = new PaymentService(_factory, NullLogger<PaymentService>.Instance);
    }

    // ========== 1. УСПЕШНЫЕ СЦЕНАРИИ ==========

    [Fact]
    public async Task InitiatePaymentAsync_WhenCardPayment_ShouldCallCardStrategy()
    {
        // Arrange
        var orderId = 1;
        var method = PaymentMethod.Card;
        var expectedResult = new PaymentResult
        {
            Success = true,
            PaymentIntentId = "pi_card_123",
            ClientSecret = "secret_card_123",
            RedirectUrl = "https://payment-gateway.com/card/pay/1"
        };

        _cardStrategyMock
            .Setup(x => x.InitiatePaymentAsync(orderId, It.IsAny<decimal>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _paymentService.InitiatePaymentAsync(orderId, 100m, method);

        // Assert
        result.Should().BeEquivalentTo(expectedResult);
        _cardStrategyMock.Verify(x => x.InitiatePaymentAsync(orderId, It.IsAny<decimal>(), It.IsAny<CancellationToken>()), Times.Once);
        _googlePayStrategyMock.Verify(x => x.InitiatePaymentAsync(It.IsAny<int>(), It.IsAny<decimal>(), It.IsAny<CancellationToken>()), Times.Never);
        _applePayStrategyMock.Verify(x => x.InitiatePaymentAsync(It.IsAny<int>(), It.IsAny<decimal>(), It.IsAny<CancellationToken>()), Times.Never);
        _sbpStrategyMock.Verify(x => x.InitiatePaymentAsync(It.IsAny<int>(), It.IsAny<decimal>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task InitiatePaymentAsync_WhenGooglePay_ShouldCallGooglePayStrategy()
    {
        // Arrange
        var orderId = 1;
        var method = PaymentMethod.GooglePay;
        var expectedResult = new PaymentResult
        {
            Success = true,
            PaymentIntentId = "pi_google_123",
            ClientSecret = "secret_google_123",
            RedirectUrl = "https://payment-gateway.com/google/pay/1"
        };

        _googlePayStrategyMock
            .Setup(x => x.InitiatePaymentAsync(orderId, It.IsAny<decimal>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _paymentService.InitiatePaymentAsync(orderId, 100m, method);

        // Assert
        result.Should().BeEquivalentTo(expectedResult);
        _googlePayStrategyMock.Verify(x => x.InitiatePaymentAsync(orderId, It.IsAny<decimal>(), It.IsAny<CancellationToken>()), Times.Once);
        _cardStrategyMock.Verify(x => x.InitiatePaymentAsync(It.IsAny<int>(), It.IsAny<decimal>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task InitiatePaymentAsync_WhenApplePay_ShouldCallApplePayStrategy()
    {
        // Arrange
        var orderId = 1;
        var method = PaymentMethod.ApplePay;
        var expectedResult = new PaymentResult
        {
            Success = true,
            PaymentIntentId = "pi_apple_123",
            ClientSecret = "secret_apple_123",
            RedirectUrl = "https://payment-gateway.com/apple/pay/1"
        };

        _applePayStrategyMock
            .Setup(x => x.InitiatePaymentAsync(orderId, It.IsAny<decimal>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _paymentService.InitiatePaymentAsync(orderId, 100m, method);

        // Assert
        result.Should().BeEquivalentTo(expectedResult);
        _applePayStrategyMock.Verify(x => x.InitiatePaymentAsync(orderId, It.IsAny<decimal>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task InitiatePaymentAsync_WhenSBP_ShouldCallSBPStrategy()
    {
        // Arrange
        var orderId = 1;
        var method = PaymentMethod.SBP;
        var expectedResult = new PaymentResult
        {
            Success = true,
            PaymentIntentId = "pi_sbp_123",
            ClientSecret = "secret_sbp_123",
            RedirectUrl = "https://payment-gateway.com/sbp/pay/1"
        };

        _sbpStrategyMock
            .Setup(x => x.InitiatePaymentAsync(orderId, It.IsAny<decimal>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _paymentService.InitiatePaymentAsync(orderId, 100m, method);

        // Assert
        result.Should().BeEquivalentTo(expectedResult);
        _sbpStrategyMock.Verify(x => x.InitiatePaymentAsync(orderId, It.IsAny<decimal>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task InitiatePaymentAsync_WithDifferentAmounts_ShouldPassCorrectAmount()
    {
        // Arrange
        var orderId = 1;
        var method = PaymentMethod.Card;
        var amount = 999.99m;
        var expectedResult = new PaymentResult { Success = true };

        _cardStrategyMock
            .Setup(x => x.InitiatePaymentAsync(orderId, amount, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _paymentService.InitiatePaymentAsync(orderId, amount, method);

        // Assert
        result.Should().BeEquivalentTo(expectedResult);
        _cardStrategyMock.Verify(x => x.InitiatePaymentAsync(orderId, amount, It.IsAny<CancellationToken>()), Times.Once);
    }

    // ========== 2. СЦЕНАРИИ С ОШИБКАМИ ==========

    [Fact]
    public async Task InitiatePaymentAsync_WhenStrategyFails_ShouldReturnFailedResult()
    {
        // Arrange
        var orderId = 1;
        var method = PaymentMethod.Card;
        var errorMessage = "Card payment failed: Insufficient funds";

        _cardStrategyMock
            .Setup(x => x.InitiatePaymentAsync(orderId, It.IsAny<decimal>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaymentResult
            {
                Success = false,
                ErrorMessage = errorMessage
            });

        // Act
        var result = await _paymentService.InitiatePaymentAsync(orderId, 100m, method);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be(errorMessage);
        _cardStrategyMock.Verify(x => x.InitiatePaymentAsync(orderId, It.IsAny<decimal>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task InitiatePaymentAsync_WhenStrategyThrowsException_ShouldReturnFailedResult()
    {
        // Arrange
        var orderId = 1;
        var method = PaymentMethod.Card;
        var exceptionMessage = "Payment gateway timeout";

        _cardStrategyMock
            .Setup(x => x.InitiatePaymentAsync(orderId, It.IsAny<decimal>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception(exceptionMessage));

        // Act
        var result = await _paymentService.InitiatePaymentAsync(orderId, 100m, method);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain(exceptionMessage);
        _cardStrategyMock.Verify(x => x.InitiatePaymentAsync(orderId, It.IsAny<decimal>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    // ========== 3. НЕПОДДЕРЖИВАЕМЫЕ МЕТОДЫ ==========

    [Fact]
    public async Task InitiatePaymentAsync_WhenUnsupportedMethod_ShouldReturnFailedResult()
    {
        // Arrange
        var unsupportedMethod = (PaymentMethod)999;

        // Act
        var result = await _paymentService.InitiatePaymentAsync(1, 100m, unsupportedMethod);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Unsupported payment method");
    }
} // Сводка теста: всего: 8; сбой: 0; успешно: 8; пропущено: 0; длительность: 0,8 с