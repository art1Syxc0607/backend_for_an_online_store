using Application.DTOs.Admin.User;
using Application.Commands.Admin.User;
using Application.Queries.Admin.User;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace WebApi.Controllers.Admin;

// WebAPI/Controllers/Admin/AdminUsersController.cs
[ApiController]
[Route("api/admin/users")]
[Authorize(Roles = "Admin")]
public class AdminUsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminUsersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<List<UserAdminDto>>> GetAllUsers(
        [FromQuery] string? search,
        [FromQuery] UserRole? role,
        [FromQuery] bool? isActive,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] SortUserBy sortBy = SortUserBy.CreatedAt,
        [FromQuery] bool sortDesc = true)
    {
        var query = new GetAllUsersQuery
        {
            Filter = new UserFilterDto
            {
                Search = search,
                Role = role,
                IsActive = isActive,
                PageNumber = pageNumber,
                PageSize = pageSize,
                SortBy = sortBy,
                SortDesc = sortDesc
            }
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost("{userId}/promote")]
    public async Task<IActionResult> PromoteToAdmin(int userId)
    {
        var command = new PromoteToAdminCommand
        {
            UserId = userId,
            AdminId = GetCurrentUserId()
        };

        await _mediator.Send(command);
        return NoContent();
    }

    [HttpPost("{userId}/demote")]
    public async Task<IActionResult> DemoteFromAdmin(int userId)
    {
        var command = new DemoteFromAdminCommand
        {
            UserId = userId,
            AdminId = GetCurrentUserId()
        };

        await _mediator.Send(command);
        return NoContent();
    }

    //[HttpPost("{userId}/block")]
    //public async Task<IActionResult> BlockUser(int userId, [FromBody] string? reason = null)
    //{
    //    var command = new BlockUserCommand
    //    {
    //        UserId = userId,
    //        AdminId = GetCurrentUserId(),
    //        Reason = reason
    //    };

    //    await _mediator.Send(command);
    //    return NoContent();
    //}

    //[HttpPost("{userId}/unblock")]
    //public async Task<IActionResult> UnblockUser(int userId)
    //{
    //    var command = new UnblockUserCommand
    //    {
    //        UserId = userId
    //    };

    //    await _mediator.Send(command);
    //    return NoContent();
    //}

    //[HttpGet("{userId}/orders")]
    //public async Task<ActionResult<List<OrderResponseDto>>> GetUserOrders(int userId)
    //{
    //    var query = new GetUserOrdersQuery { UserId = userId };
    //    var result = await _mediator.Send(query);
    //    return Ok(result);
    //}

    private int GetCurrentUserId()
    {
        var claim = User.FindFirst("userId") ?? User.FindFirst(ClaimTypes.NameIdentifier);
        return int.Parse(claim!.Value);
    }
}