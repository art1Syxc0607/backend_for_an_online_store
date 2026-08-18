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

// Infrastructure/Services/Payment/Strategies/SBPPaymentStrategy.cs
public class SBPPaymentStrategy : IPaymentStrategy
{
    private readonly ILogger<SBPPaymentStrategy> _logger;

    public SBPPaymentStrategy(ILogger<SBPPaymentStrategy> logger)
    {
        _logger = logger;
    }

    public PaymentMethod Method => PaymentMethod.SBP;

    public async Task<PaymentResult> InitiatePaymentAsync(int orderId, decimal amount, CancellationToken ct = default)
    {
        try
        {
            // Интеграция с СБП (Система быстрых платежей)
            await Task.Delay(100, ct);

            return new PaymentResult
            {
                Success = true,
                PaymentIntentId = $"sbp_{orderId}_{Guid.NewGuid():N}",
                ClientSecret = $"secret_sbp_{Guid.NewGuid():N}",
                RedirectUrl = $"https://payment-gateway.com/sbp/pay/{orderId}"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SBP payment failed for order {OrderId}", orderId);
            return new PaymentResult
            {
                Success = false,
                ErrorMessage = $"SBP payment failed: {ex.Message}"
            };
        }
    }
}
