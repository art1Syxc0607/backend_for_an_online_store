using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Order;

public record CreateOrderDto
{
    [Required]
    public List<OrderItemDto> Items { get; init; } = new();
    [Required]
    public string shippingAddress { get; init; }
}
