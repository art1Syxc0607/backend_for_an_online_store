using Application.DTOs.Order;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces;

public interface IPaymentService
{
    /// <summary>
    /// Инициирует платеж для заказа
    /// </summary>
    /// <returns>URL для перенаправления на страницу оплаты или PaymentIntentId</returns>
    Task<PaymentResult> InitiatePaymentAsync(int orderId, decimal amount ,PaymentMethod method, CancellationToken ct);

    /// <summary>
    /// Подтверждает оплату после возврата с платежного шлюза
    /// </summary>
    Task<PaymentConfirmation> ConfirmPaymentAsync(string paymentIntentId, CancellationToken ct);

    /// <summary>
    /// Возвращает средства за заказ
    /// </summary>
    Task<PaymentRefund> RefundPaymentAsync(int orderId, decimal amount, CancellationToken ct);
}