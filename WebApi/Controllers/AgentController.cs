// WebApi/Controllers/AgentController.cs
using Application.DTOs.Agent;
using Application.Interfaces;
using Infrastructure.Services.Agent;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WebApi.Controllers;

[ApiController]
[Route("api/agent")]
public class AgentController : ControllerBase
{
    private const string GuestIdCookieName = "guest_id";
    private readonly AgentService _agentService;
    private readonly ILogger<AgentController> _logger;

    public AgentController(
        AgentService agentService,
        ILogger<AgentController> logger)
    {
        _agentService = agentService;
        _logger = logger;
    }

    // ═══════════════════════════════════════════
    // Chat (для всех — user и guest)
    // ═══════════════════════════════════════════
    [HttpPost("chat")]
    public async Task<ActionResult<ChatResponse>> Chat(
        [FromBody] ChatDto dto,
        CancellationToken ct)
    {
        var identity = GetIdentity();

        // ✅ Привязываем сессию к текущему пользователю
        var request = new ChatRequest
        {
            Message = dto.Message,
            SessionId = dto.SessionId,

            UserId = identity.UserId,
            GuestId = identity.GuestId
        };

        var response = await _agentService.ChatAsync(request, ct);

        // ✅ Устанавливаем куку ТОЛЬКО для гостя
        // и ТОЛЬКО если её ещё нет
        if (identity.UserId == null
            && identity.GuestId != null
            && !Request.Cookies.ContainsKey(GuestIdCookieName))
        {
            SetGuestIdCookie(identity.GuestId);

            _logger.LogInformation(
                "Guest cookie set: {GuestId}", identity.GuestId);
        }

        return Ok(response);
    }

    // ═══════════════════════════════════════════
    // Список диалогов (для всех)
    // ═══════════════════════════════════════════
    [HttpGet("conversations")]
    [AllowAnonymous]
    public async Task<ActionResult<List<ConversationDto>>> GetConversations(
        CancellationToken ct)
    {
        var identity = GetIdentity();

        // ✅ Авторизованный
        if (identity.UserId.HasValue)
        {
            var conversations = await _agentService
                .GetConversationsAsync(identity.UserId.Value, ct);
            return Ok(conversations);
        }

        // ✅ Гость (по куке)
        if (identity.GuestId != null)
        {
            var conversations = await _agentService
                .GetGuestConversationsAsync(identity.GuestId, ct);
            return Ok(conversations);
        }

        return Ok(new List<ConversationDto>());
    }

    // ═══════════════════════════════════════════
    // Детали диалога
    // ═══════════════════════════════════════════
    [HttpGet("conversations/{sessionId}")]
    [AllowAnonymous]
    public async Task<ActionResult<ConversationDetailDto>> GetConversation(
        string sessionId,
        CancellationToken ct)
    {
        var identity = GetIdentity();

        var conversation = await _agentService.GetConversationAsync(
            identity.UserId, identity.GuestId, sessionId, ct);

        return Ok(conversation);
    }

    // изменить заголовок
    [HttpPut("conversations/{sessionId}/title")]
    [AllowAnonymous]
    public async Task<IActionResult> ChangeTitle(
        string sessionId,
        [FromBody] ChangeTitleRequest request,
        CancellationToken ct)
    {
        var identity = GetIdentity();

        await _agentService.ChangeTitleOfConversationByIdAsync(
            identity.UserId,
            identity.GuestId,
            sessionId,
            request.NewTitle,
            ct);

        return NoContent();  // ✅ 204
    }

    // ═══════════════════════════════════════════
    // Удалить диалог
    // ═══════════════════════════════════════════
    [HttpDelete("conversations/{conversationId}")]
    [AllowAnonymous]
    public async Task<IActionResult> DeleteConversation(
        string conversationId,
        CancellationToken ct)
    {
        var identity = GetIdentity();

        // ✅ Выбираем метод по identity
        bool deleted;

        if (identity.UserId.HasValue)
        {
            deleted = await _agentService.DeleteUserConversationAsync(
                identity.UserId.Value, conversationId, ct);
        }
        else if (!string.IsNullOrWhiteSpace(identity.GuestId))
        {
            deleted = await _agentService.DeleteGuestConversationAsync(
                identity.GuestId, conversationId, ct);
        }
        else
        {
            return Unauthorized();
        }

        if (!deleted)
            return NotFound();

        return NoContent();
    }

    private int GetCurrentUserId()
    {
        var claim = User.FindFirst("userId")?.Value
            ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(claim) || !int.TryParse(claim, out var userId))
            throw new UnauthorizedAccessException("User ID not found in token");

        return userId;
    }

    private (int? UserId, string? GuestId) GetIdentity()
    {
        // 1. Авторизованный
        if (User.Identity?.IsAuthenticated == true)
        {
            var claim = User.FindFirst("userId")?.Value
                ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (int.TryParse(claim, out var userId))
                return (userId, null);
        }

        // 2. Гость — читаем cookie
        var guestId = Request.Cookies[GuestIdCookieName];

        // Если cookie нет — генерируем новый
        if (string.IsNullOrWhiteSpace(guestId))
            guestId = $"guest-{Guid.NewGuid():N}";

        return (null, guestId);
    }

    private void SetGuestIdCookie(string guestId)
    {
        Response.Cookies.Append(GuestIdCookieName, guestId, new CookieOptions
        {
            HttpOnly = true,
            Secure = Request.IsHttps,  // ← true для HTTPS
            SameSite = SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.AddDays(30),
            Path = "/",
            IsEssential = true
        });
    }
}