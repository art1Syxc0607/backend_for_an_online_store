using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Cart;

public record class CartResponseDto
{
    public int Id { get; init; }
    public int UserId { get; init; }
    public DateTime UpdatedAt { get; init; }
    public decimal TotalPrice { get; init; }
    public List<CartItemDto> Items { get; init; } = new();
}
