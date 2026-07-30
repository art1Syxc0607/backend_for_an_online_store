using Application.Commands.Product;
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
using System.Xml.Linq;
using WebApi.Interfaces;
using WebApi.DTOs.Product;

namespace WebApi.Controllers;


[Route("api/product")]
[ApiController]
public class ProductController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IFileStorageService _fileStorageService;
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ProductController(IMediator mediator, IFileStorageService fileStorageService,
        IProductRepository productRepository, IUnitOfWork unitOfWork)
    {
        _mediator = mediator;
        _fileStorageService = fileStorageService;
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }


    [HttpGet]
    public async Task<ActionResult<List<ProductResponseDto>?>> GetAllProductsAsync()
    {
        var command = new GetAllProductsCommand();

        return await _mediator.Send(command);
    }

    [Authorize(Roles = "admin")]
    [HttpPost]
    public async Task<ActionResult<int>> AddProduct([FromForm] AddProductDto dto)
    {
        var command = new AddProductCommand
        {
            Name = dto.Name,
            Price = dto.Price,
            StockQuantity = dto.StockQuantity,
            CategoryId = dto.CategoryId,
            Description = dto.Description,
            ImageStream = dto.ImageFile?.OpenReadStream(),
            ImageFileName = dto.ImageFile?.FileName,
            ImageContentType = dto.ImageFile?.ContentType
        };

        var productId = await _mediator.Send(command);
        return productId;
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

    [HttpPost("{productId}/image")]
    public async Task<ActionResult<string>> UploadImage(int productId, [FromForm] IFormFile file)
    {
        // 1. Проверка файла
        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded");

        // 2. Проверка размера (5MB)
        if (file.Length > 5 * 1024 * 1024)
            return BadRequest("File size exceeds 5MB");

        // 3. Проверка типа
        var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp", "image/gif" };
        if (!allowedTypes.Contains(file.ContentType))
            return BadRequest($"Invalid file type. Allowed: {string.Join(", ", allowedTypes)}");

        // 4. Создаем команду
        var command = new UploadProductImageCommand
        {
            ProductId = productId,
            FileStream = file.OpenReadStream(),
            FileName = file.FileName,
            ContentType = file.ContentType
        };

        // 5. Отправляем через MediatR
        var imageUrl = await _mediator.Send(command);

        return Ok(new { imageUrl });
    }

    [HttpDelete("{productId}/image")]
    public async Task<IActionResult> DeleteImage(int productId)
    {
        var ct = CancellationToken.None;
        var product = await _productRepository.GetByIdAsync(productId, ct);
        if (product == null) return NotFound();

        // Удаляем из хранилища
        await _fileStorageService.DeleteFileAsync(product.ImageUrl); // логика проверки внутри

        // Удаляем URL из сущности
        product.ClearImageUrl();

        await _unitOfWork.SaveChangesAsync();

        return NoContent();
    }

    //[Authorize(Roles = "Admin")]
    //[HttpPost("upload-image")]
    //public async Task<IActionResult> UploadImage(int productId, [FromForm] IFormFile file)
    //{
    //    var imageUrl = await _imageService.SaveImageAsync(productId, file);

    //    var ct = new CancellationTokenSource().Token;
    //    // Сохраняем URL в БД (в Product.ImageUrl)
    //    var product = await _productRepository.GetByIdAsync(productId, ct);

    //    if (product == null) throw new Exception("No such product");

    //    product.SetImageUrl(imageUrl);
    //    await _unitOfWork.SaveChangesAsync();

    //    return Ok(new { imageUrl });
    //}


    // ========== Фильтрация и Поиск, Сортировка, Плагинация ==========
    [HttpGet]
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
