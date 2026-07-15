using Application.Commands.User;
using Application.DTOs.User;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Application.DTOs.Cart;
using System.Security.Claims;
using Application.Commands.Cart;


namespace WebApi.Controllers;

[Authorize]
[Route("api/cart")]
[ApiController]
public class CartController : ControllerBase
{
    private readonly IMediator _mediator;

    public CartController(IMediator mediator)
        => _mediator = mediator;

    [HttpPost("add")]
    public async Task<IActionResult> AddToCart([FromBody] AddCartItemDto request)
    {
        var command = new AddToCartCommand
        {
            productId = request.productId,
            countOfProduct = request.countOfProduct,
            userId = GetCurrentUserId(),
        };
        await _mediator.Send(command);

        return NoContent();
    }

    // 1. Удалить весь товар из корзины
    [HttpDelete("items/{productId}")]
    public async Task<IActionResult> RemoveItemFromCart(int productId)
    {
        var command = new RemoveFromCartCommand
        {
            UserId = GetCurrentUserId(),
            ProductId = productId
        };

        await _mediator.Send(command);

        return NoContent();
    }

    [HttpPut("items")]
    public async Task<IActionResult> UpdateQuantity([FromBody] UpdateQuantityDto dto)
    {
        var command = new UpdateCartItemQuantityCommand
        {
            UserId = GetCurrentUserId(),
            ProductId = dto.ProductId,
            NewQuantity = dto.Quantity,
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

