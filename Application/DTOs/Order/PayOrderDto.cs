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

public class InitiatePaymentDto
{
    [Required(ErrorMessage = "Order ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Invalid order ID")]
    public int OrderId { get; init; }

    [Required(ErrorMessage = "Payment method is required")]
    public PaymentMethod Method { get; init; }

    [MaxLength(1000, ErrorMessage = "Return URL cannot exceed 1000 characters")]
    public string? ReturnUrl { get; init; }
}

public class ConfirmPaymentDto
{
    [Required(ErrorMessage = "Payment intent ID is required")]
    [MaxLength(200, ErrorMessage = "Payment intent ID cannot exceed 200 characters")]
    public string PaymentIntentId { get; init; } = string.Empty;
}