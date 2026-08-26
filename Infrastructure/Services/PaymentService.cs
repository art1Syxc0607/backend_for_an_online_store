using Application.DTOs.Order;
using Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infrastructure.Services.Payment;
using Infrastructure.Repositories;

namespace Infrastructure.Services;

// Infrastructure/Services/PaymentService.cs
public class PaymentService : IPaymentService
{
    private readonly IPaymentStrategyFactory _strategyFactory;
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(IPaymentStrategyFactory strategyFactory, ILogger<PaymentService> logger)
    {
        _strategyFactory = strategyFactory;
        _logger = logger;
    }

    public async Task<PaymentResult> InitiatePaymentAsync(int orderId, decimal amount, PaymentMethod method, CancellationToken ct = default)
    {
        try
        {
            var strategy = _strategyFactory.GetStrategy(method);
            return await strategy.InitiatePaymentAsync(orderId, amount, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Payment initiation failed for order {OrderId}, method {Method}", orderId, method);

            return new PaymentResult
            {
                Success = false,
                ErrorMessage = $"Payment initiation failed: {ex.Message}"
            };
        }
    }

    public async Task<PaymentConfirmation> ConfirmPaymentAsync(string paymentIntentId, CancellationToken ct = default)
    {
        try
        {
            // Здесь проверка статуса платежа через платежный шлюз

            await Task.Delay(1000);
            // Для демонстрации:
            return new PaymentConfirmation
            {
                Success = true,
                TransactionId = paymentIntentId
            };
        }
        catch (Exception ex)
        {
            return new PaymentConfirmation
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<PaymentRefund> RefundPaymentAsync(int orderId, decimal amount, CancellationToken ct = default)
    {
        try
        {
            // Здесь возврат средств
            await Task.Delay(1000);

            return new PaymentRefund
            {
                Success = true,
                RefundId = $"ref_{orderId}_{Guid.NewGuid():N}"
            };
        }
        catch (Exception ex)
        {
            return new PaymentRefund
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }
}
