using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Admin.Product;

public class LowStockProductDto
{
    public int Id { get; init; }
    public string Name { get; init; }
    public string? Description { get; init; }
    public decimal Price { get; init; }
    public decimal PurchasePrice { get; init; }
    public int StockQuantity { get; init; }
    public int ReservedQuantity { get; init; }
    public int AvailableQuantity { get; init; }
    public DateTime CreatedAt { get; init; }
    public string? CategoryName { get; init; }
    public List<string> ImageUrls { get; init; } = new();
    public int OrdersCount { get; init; } // сколько раз заказывали
}