using Application.DTOs.Order;
using Domain.Enums;

namespace Application.Interfaces;

public interface IPaymentStrategy
{
    Task<PaymentResult> InitiatePaymentAsync(int orderId, decimal amount, CancellationToken ct = default);
    PaymentMethod Method { get; }
}