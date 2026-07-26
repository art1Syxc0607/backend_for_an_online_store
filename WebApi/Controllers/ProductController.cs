using Application.Commands.Product;
using Application.DTOs.Order;
using Application.DTOs.Product;
using Application.Enums;
using Application.Queries.Product;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Xml.Linq;

namespace WebApi.Controllers;


[Route("api/product")]
[ApiController]
public class ProductController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProductController(IMediator mediator)
        => _mediator = mediator;

    [HttpGet]
    public async Task<ActionResult<List<ProductResponseDto>?>> GetAllProductsAsync()
    {
        var command = new GetAllProductsCommand();

        return await _mediator.Send(command);
    }

    [Authorize(Roles = "admin")]
    [HttpPost]
    public async Task<ActionResult<int>> AddProduct([FromBody] AddProductDto dto)
    {
        var command = new AddProductCommand
        {
            Name = dto.Name,
            Price = dto.Price,
            StockQuantity = dto.StockQuantity,
            CategoryId = dto.CategoryId,
        };

        return await _mediator.Send(command);
    }


    [Authorize(Roles = "admin")]
    [HttpDelete("{productId}")]
    public async Task<IActionResult> DeleteProduct(int productId)
    {
        var command = new DeleteProductCommand
        {
            Id = productId
        };

        await _mediator.Send(command);

        return Ok();
    }

    [Authorize(Roles = "admin")]
    [HttpPut]
    public async Task<IActionResult> UpdateProduct([FromBody] UpdateProductDto dto)
    {
        var command = new UpdateProductCommand
        {
            ProductId = dto.Id,
            Price = dto.Price,
            StockQuantity = dto.StockQuantity,
            CategoryId = dto.CategoryId,
            Name = dto.Name,
        };

        await _mediator.Send(command);

        return Ok();
    }

    // ========== Фильтрация и Поиск, Сортировка ==========
    [HttpGet]
    public async Task<ActionResult<List<ProductResponseDto>>> GetProductsFilter([FromBody] ProductFilterDto dto,
        [FromQuery] SortBy? sortBy = SortBy.Name, [FromQuery] bool sortDesc = true)
    {
        var command = new GetProductsFilterCommand
        {
            SearchText = dto.SearchText,
            CategoryId = dto.CategoryId,
            PriceLimit = dto.PriceLimit,
            OnlyAvailable = dto.OnlyAvailable,
            SortBy = sortBy,
            SortDesc = sortDesc
        };

        return await _mediator.Send(command);
    }

    private int GetCurrentUserId()
    {
        var claim = User.FindFirst("userId") ?? User.FindFirst(ClaimTypes.NameIdentifier);
        return int.Parse(claim!.Value);
    }
}
