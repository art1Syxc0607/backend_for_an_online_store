using Application.DTOs.Cart;
using Application.Interfaces;
using MediatR;
using Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace Application.Queries.Cart;

public class GetCartQueryHandler : IRequestHandler<GetCartQuery, CartResponseDto>
{
    private readonly ICartRepository _cartRepository;

    public GetCartQueryHandler(ICartRepository cartRepository) =>
        _cartRepository = cartRepository;

    public async Task<CartResponseDto> Handle(GetCartQuery query, CancellationToken ct)
    {
        var cart = await _cartRepository.GetByUserIdAsync(query.UserId);

        if (cart == null) throw new DomainException("No such User or cart");

        var result = new CartResponseDto
        {
            Id = cart.Id,
            UserId = cart.UserId,
            UpdatedAt = cart.UpdatedAt,
            TotalPrice = cart.Items.Sum(i => i.Product.Price * i.Quantity),
            Items = cart.Items.Select(ci => new CartItemDto
            {
                Id = ci.Id,
                CartId = ci.CartId,
                ProductId = ci.ProductId,
                ProductName = ci.Product.Name,
                ProductPrice = ci.Product.Price,
                Quantity = ci.Quantity,
                AvailableStock = ci.Product.AvailableQuantity
            }).ToList()
        };

        return result;
    }
}
