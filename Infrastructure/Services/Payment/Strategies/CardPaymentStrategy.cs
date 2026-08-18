using Application.DTOs.Order;
using Application.Interfaces;
using Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services.Payment.Strategies;

// Infrastructure/Services/Payment/Strategies/CardPaymentStrategy.cs
public class CardPaymentStrategy : IPaymentStrategy
{
    private readonly ILogger<CardPaymentStrategy> _logger;

    public CardPaymentStrategy(ILogger<CardPaymentStrategy> logger)
    {
        _logger = logger;
    }

    public PaymentMethod Method => PaymentMethod.Card;

    public async Task<PaymentResult> InitiatePaymentAsync(int orderId, decimal amount, CancellationToken ct = default)
    {
        try
        {
            // Реальная интеграция с платежным шлюзом для карт
            // var options = new PaymentIntentCreateOptions
            // {
            //     Amount = (long)(amount * 100),
            //     Currency = "rub",
            //     PaymentMethodTypes = new List<string> { "card" },
            //     Metadata = new Dictionary<string, string> { { "orderId", orderId.ToString() } }
            // };
            // var service = new PaymentIntentService();
            // var paymentIntent = await service.CreateAsync(options, cancellationToken: ct);

            // Демо-реализация
            await Task.Delay(100, ct);

            return new PaymentResult
            {
                Success = true,
                PaymentIntentId = $"card_{orderId}_{Guid.NewGuid():N}",
                ClientSecret = $"secret_card_{Guid.NewGuid():N}",
                RedirectUrl = $"https://payment-gateway.com/card/pay/{orderId}"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Card payment failed for order {OrderId}", orderId);
            return new PaymentResult
            {
                Success = false,
                ErrorMessage = $"Card payment failed: {ex.Message}"
            };
        }
    }
}
