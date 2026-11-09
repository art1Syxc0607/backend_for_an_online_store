using Application.Commands.Favorite;
using Application.DTOs.Favorite;
using Application.Queries.Favorite;
using Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WebApi.Controllers;

[Authorize]
[Route("api/favorites")]
[ApiController]
public class FavoritesController : ControllerBase
{
    private readonly IMediator _mediator;

    public FavoritesController(IMediator mediator)
        => _mediator = mediator;

    /// <summary>
    /// Добавить товар в избранное.
    /// </summary>
    [HttpPost("{productId}")]
    public async Task<IActionResult> AddToFavorites(int productId)
    {
        var userId = GetCurrentUserId();

        var command = new AddToFavoritesCommand
        {
            UserId = userId,
            ProductId = productId,
        };

        await _mediator.Send(command);

        return NoContent();
    }

    /// <summary>
    /// Удалить товар из избранного.
    /// </summary>
    [HttpDelete("{productId}")]
    public async Task<IActionResult> RemoveFromFavorites(int productId)
    {
        var userId = GetCurrentUserId();

        var command = new RemoveFromFavoritesCommand
        {
            UserId = userId,
            ProductId = productId,
        };

        await _mediator.Send(command);

        return NoContent();
    }

    /// <summary>
    /// Получить список избранных товаров текущего пользователя.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<Application.Common.PagedResult<FavoriteDto>>> GetFavorites(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        var userId = GetCurrentUserId();

        var query = new GetUserFavoritesQuery
        {
            UserId = userId,
            PageNumber = pageNumber,
            PageSize = pageSize,
        };

        var result = await _mediator.Send(query);

        return Ok(result);
    }

    /// <summary>
    /// Проверить, добавлен ли товар в избранное текущим пользователем.
    /// </summary>
    [HttpGet("{productId}/check")]
    public async Task<ActionResult<IsFavoritedDto>> IsFavorited(int productId)
    {
        var userId = GetCurrentUserId();

        var query = new IsProductFavoritedQuery
        {
            UserId = userId,
            ProductId = productId,
        };

        var result = await _mediator.Send(query);

        return Ok(new IsFavoritedDto { IsFavorited = result });
    }

    private int GetCurrentUserId()
    {
        var claim = User.FindFirst("userId") ?? User.FindFirst(ClaimTypes.NameIdentifier);
        return int.Parse(claim!.Value);
    }
}
