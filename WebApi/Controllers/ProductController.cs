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

    //[HttpPost("{productId}/image")] // хочу одним методом чтобы и фото и видео и несколько их
    //public async Task<ActionResult<string>> UploadImage(int productId, [FromForm] IFormFile file)
    //{
    //    // 1. Проверка файла
    //    if (file == null || file.Length == 0)
    //        return BadRequest("No file uploaded");

    //    // 2. Проверка размера (5MB)
    //    if (file.Length > 5 * 1024 * 1024)
    //        return BadRequest("File size exceeds 5MB");

    //    // 3. Проверка типа
    //    var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp", "image/gif" };
    //    if (!allowedTypes.Contains(file.ContentType))
    //        return BadRequest($"Invalid file type. Allowed: {string.Join(", ", allowedTypes)}");

    //    // 4. Создаем команду
    //    var command = new UploadProductImageCommand
    //    {
    //        ProductId = productId,
    //        FileStream = file.OpenReadStream(),
    //        FileName = file.FileName,
    //        ContentType = file.ContentType
    //    };

    //    // 5. Отправляем через MediatR
    //    var imageUrl = await _mediator.Send(command);

    //    return Ok(new { imageUrl });
    //}

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


    [Authorize(Roles = "Admin")]
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

    [Authorize(Roles = "Admin")]
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
