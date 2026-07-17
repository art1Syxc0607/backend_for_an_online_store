using System.ComponentModel.DataAnnotations;
using Application.DTOs.Order;
using MediatR;

namespace Application.Commands.Order;



public class CreateOrderCommand : IRequest<int>
{
    public List<OrderItemDto> Items { get; init; } = new();
    public int UserId { get; init; }
    [Required]
    public string shippingAddress { get; init; }
}
