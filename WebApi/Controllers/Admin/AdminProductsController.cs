using Application.Commands.Product;
using Application.Queries.Admin.Product;
using Application.DTOs.Admin.Product;
using Application.DTOs.Product;
using Application.Interfaces;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        // Проверка файлов
        if (dto.Files != null)
        {
            const int maxFiles = 8;
            if (dto.Files.Count > maxFiles)
                return BadRequest($"Maximum {maxFiles} files allowed");

            foreach (var file in dto.Files)
            {
                if (file.Length == 0)
                    return BadRequest($"File '{file.FileName}' is empty");

                if (file.Length > 500 * 1024 * 1024) // 500MB
                    return BadRequest($"File '{file.FileName}' exceeds 500MB");

                if (!IsValidFileType(file.ContentType))
                    return BadRequest($"Unsupported file type: {file.ContentType}");
            }
        }

        var command = new AddProductCommand
        {
            Name = dto.Name,
            Price = dto.Price,
            StockQuantity = dto.StockQuantity,
            CategoryId = dto.CategoryId,
            Description = dto.Description,
            Files = dto.Files?.Select(f => new FileUploadDto
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
            StockQuantity = dto.StockQuantity,
            CategoryId = dto.CategoryId,
            Name = dto.Name,
        };

        await _mediator.Send(command);

        return Ok();
    }


    [HttpPost("{productId}/files")]
    public async Task<ActionResult<List<FileUploadResponseDto>>> UploadFiles(
    int productId,
    [FromForm] List<IFormFile> files)
    {
        // 1. Проверка количества
        if (files == null || !files.Any())
            return BadRequest("No files uploaded");

        const int maxFiles = 8;
        if (files.Count > maxFiles)
            return BadRequest($"Maximum {maxFiles} files allowed");

        // 2. Проверка каждого файла
        foreach (var file in files)
        {
            if (file.Length == 0)
                return BadRequest($"File '{file.FileName}' is empty");

            if (file.Length > 500 * 1024 * 1024) // 500MB
                return BadRequest($"File '{file.FileName}' exceeds 500MB");

            var isValidType = IsValidFileType(file.ContentType);
            if (!isValidType)
                return BadRequest($"File '{file.FileName}' has unsupported type: {file.ContentType}");
        }

        // 3. Команда
        var command = new UploadProductFilesCommand
        {
            ProductId = productId,
            Files = files.Select(f => new FileUploadDto
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

    private bool IsValidFileType(string contentType)
    {
        var allowedTypes = new HashSet<string>
        {
            // Images
            "image/jpeg", "image/png", "image/webp", "image/gif", "image/svg+xml",
            // Videos
            "video/mp4", "video/webm", "video/ogg", "video/quicktime",
            // Documents
            "application/pdf", "application/msword",
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
        };
        return allowedTypes.Contains(contentType);
    }


}
