using Application.Commands.User;
using Application.DTOs.User;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
//using Application.Queries.GetProfile;

namespace WebApi.Controllers;


[ApiController]
[Route("api/auth")]
public class UserController : ControllerBase
{
    private readonly IMediator _mediator;

    public UserController(IMediator mediator)
        => _mediator = mediator;

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterDto registerDto)
    {
        var command = new RegisterCommand
        {
            Email = registerDto.Email,
            UserName = registerDto.UserName,
            Password = registerDto.Password,
        };

        var result = await _mediator.Send(command);

        return Ok(result);
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto loginDto)
    {
        var command = new RegisterCommand
        {
            Email = loginDto.Email,
            Password = loginDto.Password,
        };

        var result = await _mediator.Send(command);

        return Ok(result);
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        var userId = GetCurrentUserId();
        var command = new ChangePasswordCommand
        {
            UserId = userId,
            CurrentPassword = dto.CurrentPassword,
            NewPassword = dto.NewPassword
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

