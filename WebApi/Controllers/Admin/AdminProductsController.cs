using Application.Commands.Product;
using Application.Common;
using Application.Queries.Admin.Product;
using Application.DTOs.Admin.Product;
using Application.DTOs.Product;
using Application.DTOs.File;
using Application.Enums;
using Application.Interfaces;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using WebApi.Common;
using WebApi.DTOs.Product;

namespace WebApi.Controllers.Admin;

[Route("api/admin/product")]
[Authorize(Roles = "Admin")]
public class AdminProductsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IFileStorageService _fileStorageService;

    public AdminProductsController(IMediator mediator, IFileStorageService fileStorageService)
    {
        _mediator = mediator;
        _fileStorageService = fileStorageService;
    }

    [HttpPost]
    public async Task<ActionResult<int>> AddProduct([FromForm] AddProductDto dto)
    {
        var validationError = FileUploadValidation.Validate(dto.Files);
        if (validationError != null)
            return BadRequest(validationError);

        var command = new AddProductCommand
        {
            Name = dto.Name,
            Price = dto.Price,
            PurchasePrice = dto.PurchasePrice,
            StockQuantity = dto.StockQuantity,
            CategoryId = dto.CategoryId,
            Description = dto.Description,
            Files = dto.Files?.Select(f => new Application.DTOs.File.FileUploadDto
            {
                Stream = f.OpenReadStream(),
                FileName = f.FileName,
                ContentType = f.ContentType,
                Length = f.Length,
            }).ToList(),


            //ImageFileName = dto.ImageFile?.FileName,
            //ImageContentType = dto.ImageFile?.ContentType


        };

        var productId = await _mediator.Send(command);
        return productId;
    }


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


    [HttpPut]
    public async Task<IActionResult> UpdateProduct([FromBody] UpdateProductDto dto)
    {
        var command = new UpdateProductCommand
        {
            ProductId = dto.Id,
            Price = dto.Price,
            PurchasePrice = dto.PurchasePrice,
            StockQuantity = dto.StockQuantity,
            ReservedQuantity = dto.ReservedQuantity,
            CategoryId = dto.CategoryId,
            Name = dto.Name,
            Description = dto.Description,
            Sku = dto.Sku
        };

        await _mediator.Send(command);

        return Ok();
    }


    [HttpPost("{productId}/files")]
    public async Task<ActionResult<List<Application.DTOs.File.FileUploadResponseDto>>> UploadFiles(
    int productId,
    [FromForm] List<IFormFile> files)
    {
        var validationError = FileUploadValidation.Validate(files, required: true);
        if (validationError != null)
            return BadRequest(validationError);

        // 2. Команда
        var command = new UploadProductFilesCommand
        {
            ProductId = productId,
            Files = files.Select(f => new Application.DTOs.File.FileUploadDto
            {
                Stream = f.OpenReadStream(),
                FileName = f.FileName,
                ContentType = f.ContentType,
                Length = f.Length
            }).ToList()
        };

        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpDelete("{productId}/files")]
    public async Task<ActionResult<DeleteFilesResponseDto>> DeleteFiles(
    int productId,
    [FromBody] DeleteFilesRequestDto request)
    {
        if (request.FileUrls == null || !request.FileUrls.Any())
            return BadRequest("No file URLs provided");

        var command = new DeleteFilesCommand
        {
            ProductId = productId,
            FileUrls = request.FileUrls
        };

        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AdminProductDto>> GetProductById(int id)
    {
        var query = new GetAdminProductByIdQuery { Id = id };
        var result = await _mediator.Send(query);

        if (result == null)
            return NotFound();

        return Ok(result);
    }

    [HttpGet("filtered")]
    public async Task<ActionResult<PagedResult<AdminProductDto>>> GetFilteredProducts(
        [FromQuery] AdminProductFilterDto dto)
    {
        var query = new GetAdminProductsQuery
        {
            SearchText = dto.SearchText,
            CategoryId = dto.CategoryId,
            PriceLimitMax = dto.PriceLimitMax,
            PriceLimitMin = dto.PriceLimitMin,
            OnlyAvailable = dto.OnlyAvailable,
            PageNumber = dto.PageNumber,
            PageSize = dto.PageSize,
            SortBy = dto.SortBy,
            SortDesc = dto.SortDesc,
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("low-stock/{limit}")]
    public async Task<ActionResult<List<LowStockProductDto>>> GetLowStockProducts(
        [FromRoute] int limit,
        [FromQuery] bool includeReserved = true,
        [FromQuery] int? categoryId = null,
        [FromQuery] string? search = null)
    {
        if (limit <= 0)
            return BadRequest("Limit must be greater than 0");

        var query = new GetLowStockProductsQuery
        {
            Limit = limit,
            IncludeReserved = includeReserved,
            CategoryId = categoryId,
            Search = search
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

}
