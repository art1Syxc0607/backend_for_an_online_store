using Application.Interfaces.Caching;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Application.Commands.Cart;

public class CheckoutCommand : IRequest<int> // Id of an Order
{
    [Required]
    public int UserId { get; init; }
    [Required]
    public string ShippingAddress { get; init; }
    [Required]
    public string BaseUrl { get; init; }


    // ✅ Инвалидируем популярные товары (изменились продажи)
    public IEnumerable<string> CachePrefixesToInvalidate => new[]
    {
        CacheKeys.ProductsPrefix,
        CacheKeys.PopularProductsPrefix
    };
}
