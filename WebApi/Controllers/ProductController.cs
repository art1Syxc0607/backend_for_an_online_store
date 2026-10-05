using Application.Queries.Admin.Dashboard;
using Application.Commands.Product;
using Application.Common;
using Application.DTOs.Order;
using Application.DTOs.Product;
using Application.Enums;
using Application.Interfaces;
using Application.Queries.Product;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json.Serialization;
using System.Xml.Linq;
using WebApi.DTOs.Product;
using WebApi.Interfaces;

namespace WebApi.Controllers;


[Route("api/product")]
[ApiController]
[AllowAnonymous]
public class ProductController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IFileStorageService _fileStorageService;

    public ProductController(IMediator mediator, IFileStorageService fileStorageService)
    {
        _mediator = mediator;
        _fileStorageService = fileStorageService;

    }

    [HttpGet("popularProducts")]
    public async Task<ActionResult<PagedResult<PopularProductDto>>> GetMostPopularProductsForThePeriod(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;

        var command = new GetMostPopularProductsForThePeriodCommand
        {
            LastDayOfThePeriod = now,
            FirstDayOfThePeriod = now.AddDays(-14),
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _mediator.Send(command, ct);
        return Ok(result);
    }

    [HttpGet("{productId}")]
    public async Task<ActionResult<ProductResponseDto>> GetProductAsync(int productId)
    {
        var command = new GetProductQuery
        {
            Id = productId
        };

        return await _mediator.Send(command);
    }

    [HttpGet]
    public async Task<ActionResult<List<ProductResponseDto>>> GetAllProductsAsync()
    {
        var command = new GetAllProductsCommand();

        return await _mediator.Send(command);
    }


    // ========== Фильтрация и Поиск, Сортировка, Плагинация ==========
    [HttpGet("filter")]
    public async Task<ActionResult<List<ProductResponseDto>>> GetProductsFilter([FromQuery] ProductFilterDto dto)
    {
        var command = new GetProductsFilterCommand
        {
            SearchText = dto.SearchText,
            CategoryId = dto.CategoryId,
            PriceLimitMax = dto.PriceLimitMax,
            PriceLimitMin = dto.PriceLimitMin,
            OnlyAvailable = dto.OnlyAvailable,
            PageNumber = dto.PageNumber,
            PageSize = dto.PageSize,       
            SortBy = dto.SortBy,
            SortDesc = dto.SortDesc
        };

        return await _mediator.Send(command);
    }

    private int GetCurrentUserId()
    {
        var claim = User.FindFirst("userId") ?? User.FindFirst(ClaimTypes.NameIdentifier);
        return int.Parse(claim!.Value);
    }
}
