using Domain.Entities;

namespace Application.DTOs.Cart;

public record CartItemDto
{
    public int Id { get; init; }
    public int CartId { get; init; }
    public int ProductId { get; init; }
    public string ProductName { get; init; }
    public decimal ProductPrice { get; init; }
    public int Quantity { get; init; }
    public int AvailableStock { get; init; }
}
