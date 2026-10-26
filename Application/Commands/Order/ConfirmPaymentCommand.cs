using Application.DTOs.Order;
using MediatR;
using Application.Interfaces.Caching;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Commands.Order;

// Application/Commands/Payment/ConfirmPaymentCommand.cs
public class ConfirmPaymentCommand : IRequest<PaymentConfirmation>, ICacheInvalidatingCommand
{
    [Required]
    public string PaymentIntentId { get; init; } = string.Empty;

    public int UserId { get; init; }

    [Required]
    public string BaseUrl { get; init; } = string.Empty;

    // ✅ Инвалидируем весь кэш товаров
    public IEnumerable<string> CachePrefixesToInvalidate => new[]
    {
        CacheKeys.ProductsPrefix,
        CacheKeys.PopularProductsPrefix
    };
}