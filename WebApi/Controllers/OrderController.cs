using Domain.Entities;
using Application.DTOs.Order;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Application.Queries.Order;
using System.Security.Claims;
using MediatR;

namespace WebApi.Controllers
{

    [Authorize]
    [Route("api/order")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IMediator _mediator;

        public OrderController(IMediator mediator)
            => _mediator = mediator;

        [HttpGet]
        public async Task<ActionResult<List<OrderResponseDto>>> GetOrderHistory()
        {
            var command = new GetOrderHistoryQuery
            {
                userId = GetCurrentUserId()
            };

            var history = await _mediator.Send(command);

            return history;

        }

        private int GetCurrentUserId()
        {
            var claim = User.FindFirst("userId") ?? User.FindFirst(ClaimTypes.NameIdentifier);
            return int.Parse(claim!.Value);
        }
    }
}
