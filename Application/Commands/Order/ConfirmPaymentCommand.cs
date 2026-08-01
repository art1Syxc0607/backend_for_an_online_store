using Application.DTOs.Order;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Commands.Order;

// Application/Commands/Payment/ConfirmPaymentCommand.cs
public class ConfirmPaymentCommand : IRequest<PaymentConfirmation>
{
    public string PaymentIntentId { get; init; }
}