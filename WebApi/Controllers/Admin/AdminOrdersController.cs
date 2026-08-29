using Application.Commands.Admin.Order;
using Application.Commands.Order;
using Application.DTOs.Order;
using Application.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApi.DTOs.Order;

namespace WebApi.Controllers.Admin;

[Authorize(Roles = "Admin")]
[Route("api/admin/order")]
[ApiController]
public class AdminOrdersController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminOrdersController(IMediator mediator)
        => _mediator = mediator;


    [HttpGet]
    public async Task<List<OrderResponseDto>> GetAllOrderOrFiltered([FromQuery] GetAllOrderDto dto)
    {
        var command = new GetAllOrderOrFilteredCommand
        {
            Date = dto.Date,
            DateSpan = dto.DateSpan,
            Status = dto.OrderStatus,
            UserId = dto.UserID,
            OrderSortBy = dto.OrderBy,
            SortDesc = dto.SortDesc,
            PageNumber = dto.PageNumber,
            PageSize = dto.PageSize
        };

        return await _mediator.Send(command);
    }

    
    [HttpPost("ship/{orderId}")]
    public async Task<IActionResult> ShipOrder(int orderId)
    {
        var command = new InitiateShipmentCommand { OrderId = orderId };

        await _mediator.Send(command);

        return Ok();
    }

    [HttpPost("deliver/{orderId}")]
    public async Task<IActionResult> DeliverOrder(int orderId)
    {
        var command = new DeliverOrderCommand { OrderId = orderId };

        await _mediator.Send(command);

        return Ok();
    }


}
