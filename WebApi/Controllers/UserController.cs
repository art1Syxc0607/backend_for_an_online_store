using Application.Commands.Email;
using Application.Commands.User;
using Application.DTOs.User;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
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
    public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterDto dto)
    {
        // Получаем IP-адрес
        var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

        var command = new RegisterCommand { 
            Email = dto.Email,
            UserName = dto.UserName,
            Password = dto.Password,
            UserIP = clientIp
        };
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
        //return Redirect("https://yourstore.com/email-confirmed");

        return NoContent();
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto loginDto)
    {
        // Получаем IP-адрес
        var userIP = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

        var command = new LoginCommand
        {
            Email = loginDto.Email,
            Password = loginDto.Password,
            UserIP = userIP,
        };

        var result = await _mediator.Send(command);

        return Ok(result);
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        // Получаем IP-адрес
        var userIP = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

        var userId = GetCurrentUserId();
        var command = new ChangePasswordCommand
        {
            UserId = userId,
            CurrentPassword = dto.CurrentPassword,
            NewPassword = dto.NewPassword,
            UserIP = userIP
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

