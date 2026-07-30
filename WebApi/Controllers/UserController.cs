using Application.Commands.Email;
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
    public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpGet("confirm-email")]
    public async Task<IActionResult> ConfirmEmail([FromQuery] int userId, [FromQuery] string token)
    {
        var command = new ConfirmEmailCommand
        {
            UserId = userId,
            Token = token
        };

        await _mediator.Send(command);

        // Перенаправляем на фронтенд страницу успеха
        return Redirect("https://yourstore.com/email-confirmed");
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

