using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Domain.Enums;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Order;



public record PaymentResult
{
    public bool Success { get; init; }
    public string? PaymentIntentId { get; init; }
    public string? ClientSecret { get; init; }   // для Stripe
    public string? RedirectUrl { get; init; }    // для перенаправления
    public string? ErrorMessage { get; init; }
}

public record PaymentConfirmation
{
    public bool Success { get; init; }
    public string? TransactionId { get; init; }
    public string? ErrorMessage { get; init; }
}

public record PaymentRefund
{
    public bool Success { get; init; }
    public string? RefundId { get; init; }
    public string? ErrorMessage { get; init; }
}

public record InitiatePaymentDto
{
    [Required]
    public int OrderId { get; init; }

    [Required]
    public PaymentMethod Method { get; init; }

    public string? ReturnUrl { get; init; } // куда вернуть пользователя после оплаты
}

public record ConfirmPaymentDto
{
    [Required]
    public string PaymentIntentId { get; init; }
}