using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Product;

public class ProductResponseDto
{
    public int Id { get; init; }
    public string Name { get; init; }
    public string? Description { get; init; }
    public decimal Price { get; init; }
    public int StockQuantity { get; init; }
    public int ReservedQuantity { get; init; }
    public int AvailableQuantity => StockQuantity - ReservedQuantity;

    public double AverageRating { get; init; }
    public int CountOfReviews { get; init; }

    public int AmountOfRecieved { get; init; } 
    public int AmountOfPaid { get; init; } 
    public int AmountOfCanceled { get; init; } 
    public int CountOfOrdersContainThisProduct { get; init; }

    public List<string>? ImageUrls { get; init; }
    public List<string>? VideoUrls { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }

    // Внешние ключи
    public int? CategoryId { get; init; }
}

