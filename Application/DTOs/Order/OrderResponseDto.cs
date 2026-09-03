using Application.DTOs.Cart;
using Domain.Entities;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Order;

public record OrderResponseDto
{
    public List<OrderItemDto> Items { get; init; } = new List<OrderItemDto>();

    public int OrderId { get; init; }
    public int UserId { get; init; }
    //public string ProductName { get; init; }
    public decimal TotalAmount { get; init; }
    public string ShippingAddress { get; init; }
    public OrderStatus Status { get; init; }
    public DateTime CreatedAt { get; init; }

    // info 
    public DateTime? PaidAt { get; init; }
    public DateTime? ShippedAt { get; init; }
    public DateTime? DeliveredAt { get; init; }
    public DateTime? ReceivedAt { get; init; }
    public DateTime? CancelledAt { get; init; }
}
