using System;

namespace Application.DTOs.Favorite;

public class FavoriteDto
{
    public int UserId { get; init; }
    public int ProductId { get; init; }
    public DateTime CreatedAt { get; init; }
    public string? ProductName { get; init; }
    public decimal? ProductPrice { get; init; }
    public string? ProductImageUrl { get; init; }
}
