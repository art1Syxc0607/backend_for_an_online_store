using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;
using Application.DTOs.Order;

namespace Application.DTOs.Email;

public record OrderConfirmationEmailDto
{
    public string UserEmail { get; init; }
    public string UserName { get; init; }
    public int OrderId { get; init; }
    public decimal TotalAmount { get; init; }
    public DateTime OrderDate { get; init; }
    public List<OrderItemDto> Items { get; init; }
    public string ShippingAddress { get; init; }
}