using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Order;

public record OrderItemDto
{
    [Required(ErrorMessage = "Product ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Invalid product ID")]
    public int ProductId { get; init; }

    [Required(ErrorMessage = "Quantity is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
    public int Quantity { get; init; }

    [Required(ErrorMessage = "Price is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
    public decimal PriceAtPurchase { get; init; }

    [MinLength(5, ErrorMessage = "ProductNameAtPurchase must be at least 3 characters")]
    [MaxLength(50, ErrorMessage = "ProductNameAtPurchase cannot exceed 50 characters")]
    [Required(ErrorMessage = "ProductNameAtPurchase is required")]
    public string ProductNameAtPurchase { get; init; } = string.Empty;
}
