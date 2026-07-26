using Application.DTOs.Order;
using MediatR;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Commands.Order;

public class InitiatePaymentCommand : IRequest<PaymentResult>
{
    public int UserId { get; init; }
    public int OrderId { get; init; }
    public PaymentMethod Method { get; init; }
    //public string? ReturnUrl { get; init; }
}