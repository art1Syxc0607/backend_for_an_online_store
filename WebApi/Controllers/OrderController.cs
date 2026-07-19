using Domain.Entities;
using Application.DTOs.Order;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Application.Queries.Order;
using System.Security.Claims;
using MediatR;
using Application.Commands.Order;
//using WebApi.DTOs.Order;

namespace WebApi.Controllers;


[Authorize]
[Route("api/order")]
[ApiController]
public class OrderController : ControllerBase
{
    private readonly IMediator _mediator;

    public OrderController(IMediator mediator)
        => _mediator = mediator;

    [HttpGet("history")]
    public async Task<ActionResult<List<OrderResponseDto>>> GetOrderHistory()
    {
        var command = new GetOrderHistoryQuery
        {
            userId = GetCurrentUserId()
        };

        var history = await _mediator.Send(command);

        return history;

    }

    [HttpGet("{orderId}")]
    public async Task<ActionResult<OrderResponseDto>> GetOrder(int orderId)
    {
        var command = new GetOrderQuery
        {
            UserId = GetCurrentUserId(),
            OrderId = orderId
        };

        var result = await _mediator.Send(command);


        return result;
    }


    [HttpPost]
    public async Task<ActionResult<int>> CreateOrder([FromBody] CreateOrderDto dto)
    {
        var command = new CreateOrderCommand
        {
            UserId = GetCurrentUserId(),
            Items = dto.Items,
            ShippingAddress = dto.shippingAddress,
        };

        var result = await _mediator.Send(command);

        return result;
    }

    [HttpPut]
    public async Task<IActionResult> CancelOrder([FromBody] CancelOrderDto dto)
    {
        var command = new CancelOrderCommand
        {
            UserId = GetCurrentUserId(),
            OrderId = dto.OrderId,
        };

        await _mediator.Send(command);

        return NoContent();
    }

    private int GetCurrentUserId()
    {
        var claim = User.FindFirst("userId") ?? User.FindFirst(ClaimTypes.NameIdentifier);
        return int.Parse(claim!.Value);
    }
}
