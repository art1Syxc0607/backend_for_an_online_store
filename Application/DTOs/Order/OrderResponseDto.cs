using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;
using Domain.Enums;

namespace Application.DTOs.Order;

public record OrderResponseDto
{
    public List<OrderItem> Items { get; init; } = new();

    public int Id { get; init; }
    public int UserId { get; init; }
    public decimal TotalAmount { get; init; }
    public string ShippingAddress { get; init; }
    public OrderStatus Status { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? PaidAt { get; init; }
}
