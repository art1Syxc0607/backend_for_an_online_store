using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;

namespace Application.DTOs.Product;

public class PopularProductDto
{
    public int ProductId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public decimal Price { get; init; }
    public int StockQuantity { get; init; }
    public int ReservedQuantity { get; init; }
    public int AvailableQuantity => StockQuantity - ReservedQuantity;
    public int TotalPurchases { get; init; }  // ← количество покупок за период
    public int PresenceInOrders { get; init; } // уникальные присутствие в уникальных заказах
    public int AmountOfPendingForThePeriod { get; init; }
    public int AmountOfPaidForThePeriod { get; init; }
    public int AmountOfShippedForThePeriod { get; init; }
    public int AmountOfDeliveredForThePeriod { get; init; }
    public int AmountOfReceivedForThePeriod { get; init; }
    public int AmountOfCancelledForThePeriod { get; init; }
    public List<string>? ImageUrls { get; init; } = new List<string>();
    public List<string>? VideoUrls { get; init; } = new List<string>();
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
    public DateTime EndDate { get; init; }
    public DateTime StartDate { get; init; }

    // Внешние ключи
    public int? CategoryId { get; init; }
}
