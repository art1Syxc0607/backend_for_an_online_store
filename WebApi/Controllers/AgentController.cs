// WebApi/Controllers/AgentController.cs
using Application.DTOs.Agent;
using Application.Interfaces;
using Infrastructure.Services.Agent;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[ApiController]
[Route("api/agent")]
[Authorize]
public class AgentController : ControllerBase
{
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
    // Список диалогов пользователя
    // ═══════════════════════════════════════════
    [HttpGet("conversations")]
    public async Task<ActionResult<List<ConversationDto>>> GetConversations(
        CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        var conversations = await _agentService
            .GetConversationsAsync(userId, ct);

        return Ok(conversations);
    }

    // ═══════════════════════════════════════════
    // История сообщений диалога
    // ═══════════════════════════════════════════
    [HttpGet("conversations/{sessionId}")]
    public async Task<ActionResult<ConversationDetailDto>> GetConversation(
        string sessionId,
        CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        var conversation = await _agentService
            .GetConversationAsync(userId, sessionId, ct);

        return Ok(conversation);
    }


    /// <summary>
    /// Отправить сообщение AI-помощнику.
    /// </summary>
    [HttpPost("chat")]
    public async Task<ActionResult<ChatResponse>> Chat(
        [FromBody] ChatDto dto,
        CancellationToken ct)
    {
        // ✅ Привязываем сессию к текущему пользователю
        var request = new ChatRequest
        {
            Message = dto.Message, 
            SessionId = dto.SessionId,

            UserId = GetCurrentUserId()
        };

        var response = await _agentService.ChatAsync(request, ct);
        return Ok(response);
    }

    // ═══════════════════════════════════════════
    // Удалить диалог
    // ═══════════════════════════════════════════
    [HttpDelete("conversations/{sessionId}")]
    public async Task<IActionResult> DeleteConversation(
        string sessionId,
        CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        await _agentService.DeleteConversationAsync(userId, sessionId, ct);
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
}