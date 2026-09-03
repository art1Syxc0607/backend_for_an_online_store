using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Order;

public class CreateOrderDto
{
    //[Required(ErrorMessage = "Items are required")] // before
    //[MinLength(1, ErrorMessage = "At least one item is required")]
    //public List<OrderItemDto> Items { get; init; } = new();

    [Required(ErrorMessage = "Items are required")]
    [MinLength(1, ErrorMessage = "At least one item is required")]
    public List<CreateOrderItemDto> Items { get; init; } = new();

    [Required(ErrorMessage = "Shipping address is required")]
    [MinLength(5, ErrorMessage = "Shipping address must be at least 5 characters")]
    [MaxLength(500, ErrorMessage = "Shipping address cannot exceed 500 characters")]
    public string ShippingAddress { get; init; } = string.Empty;
}