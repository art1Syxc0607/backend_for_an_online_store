using Application.DTOs.Order;
using MediatR;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Commands.Order;

// Application/Commands/Payment/ConfirmPaymentCommand.cs
public class ConfirmPaymentCommand : IRequest<PaymentConfirmation>
{
    [Required]
    public string PaymentIntentId { get; init; } = string.Empty;
    [Required]
    public string BaseUrl { get; init; } = string.Empty;
}