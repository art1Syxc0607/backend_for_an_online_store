using Application.Commands.Admin.Dashboard;
using Application.Commands.Admin.Order;
using Application.Commands.Order;
using Application.DTOs.Admin.Order;
using Application.DTOs.Order;
using Application.DTOs.Product;
using Application.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace WebApi.Controllers.Admin;

[Authorize(Roles = "Admin")]
[Route("api/admin/dashboard")]
[ApiController]
public class AdminDashboardController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminDashboardController(IMediator mediator)
        => _mediator = mediator;

    [HttpGet]
    public async Task<int> NumberOfNewOrders([FromQuery] DateSpan span)
    {
        var command = new GetNumberOfNewOrdersCommand
        {
            DateSpan = span
        };

        return await _mediator.Send(command);
    }

    [HttpGet("/revenue")]
    public async Task<RevenueForThePeriodDto> RevenueForThePeriod([FromQuery] DateTime lastDayOfThePriod, 
        [FromQuery] DateSpan span)
    {
        var command = new GetRevenueForThePeriodCommand
        {
            LastDayOfThePriod = lastDayOfThePriod,
            DateSpan = span
        };

        return await _mediator.Send(command);
    }

    [HttpGet("/popularProducts")]
    public async Task<List<PopularProductDto>> GetMostPopularProductsForThePeriod([FromQuery] DateSpan span,
        [FromQuery] DateTime lastDayOfThePriod)
    {
        var command = new GetMostPopularProductsForThePeriodCommand
        {
            Span = span,
            LastDayOfThePriod = lastDayOfThePriod
        };

        return await _mediator.Send(command);

    }
}
