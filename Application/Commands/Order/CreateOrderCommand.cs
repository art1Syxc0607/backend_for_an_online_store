using System.ComponentModel.DataAnnotations;
using Application.DTOs.Order;
using MediatR;

namespace Application.Commands.Order;



public class CreateOrderCommand : IRequest<int>
{
    [Required]
    public List<OrderItemDto> Items { get; init; } = new();
    [Required]
    public int UserId { get; init; }
    [Required]
    public string ShippingAddress { get; init; }
}
