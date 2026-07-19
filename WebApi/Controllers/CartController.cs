using Application.Commands.Cart;
using Application.Commands.User;
using Application.DTOs.Cart;
using Application.DTOs.User;
using Application.Queries.Cart;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;


namespace WebApi.Controllers;

[Authorize]
[Route("api/cart")]
[ApiController]
public class CartController : ControllerBase
{
    private readonly IMediator _mediator;

    public CartController(IMediator mediator)
        => _mediator = mediator;

    [HttpGet]
    public async Task<CartResponseDto> GetCart()
    {
        var command = new GetCartQuery { UserId = GetCurrentUserId() };

        var result = await _mediator.Send(command);

        return result;
    }

    [HttpPost("Checkout")]
    public async Task<ActionResult<int>> Checkout([FromBody] MakeCheckoutDto dto)
    {
        var command = new CheckoutCommand
        {
            UserId = GetCurrentUserId(),
            ShippingAddress = dto.shippingAddress
        };

        var id = await _mediator.Send(command);

        return id;
    }

    [HttpPost()]
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

