using Application.Commands.Cart;
using Application.Interfaces.Caching;
using MediatR;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Commands.Cart;

public class AddToCartCommand : IRequest
{
    [Required]
    public int ProductId { get; set; }
    [Required]
    public int Quantity { get; set; }
    [Required]
    public int UserId { get; set; }


    // ✅ Инвалидируем только корзину пользователя
    public IEnumerable<string> CachePrefixesToInvalidate => new[]
    {
        CacheKeys.Cart(UserId)  // ← конкретный ключ
    };

}
