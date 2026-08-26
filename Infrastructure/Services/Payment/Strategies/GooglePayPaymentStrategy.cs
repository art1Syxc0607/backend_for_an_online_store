using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.DTOs.Order;
using Application.Interfaces;
using Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services.Payment.Strategies;

// Infrastructure/Services/Payment/Strategies/GooglePayPaymentStrategy.cs
public class GooglePayPaymentStrategy : IPaymentStrategy
{
    private readonly ILogger<GooglePayPaymentStrategy> _logger;

    public GooglePayPaymentStrategy(ILogger<GooglePayPaymentStrategy> logger)
    {
        _logger = logger;
    }

    public PaymentMethod Method => PaymentMethod.GooglePay;

    public async Task<PaymentResult> InitiatePaymentAsync(int orderId, decimal amount, CancellationToken ct = default)
    {
        try
        {
            // Интеграция с Google Pay API
            await Task.Delay(100, ct);

            return new PaymentResult
            {
                Success = true,
                PaymentIntentId = $"google_{orderId}_{Guid.NewGuid():N}",
                ClientSecret = $"secret_google_{Guid.NewGuid():N}",
                RedirectUrl = $"https://payment-gateway.com/google/pay/{orderId}"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Google Pay failed for order {OrderId}", orderId);
            return new PaymentResult
            {
                Success = false,
                ErrorMessage = $"Google Pay failed: {ex.Message}"
            };
        }
    }
}