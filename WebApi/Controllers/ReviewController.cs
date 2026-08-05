using Application.Commands.Product;
using Application.Commands.Review;
using Application.Commands.User;
using Application.DTOs.Review;
using Application.DTOs.File;
using Application.Queries.Review;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebApi.DTOs.Review;

namespace WebApi.Controllers;


[Route("api/review")]
public class ReviewController : Controller
{
    private readonly IMediator _mediator;

    public ReviewController(IMediator mediator)
        => _mediator = mediator;

    [Authorize]
    [HttpGet()] 
    public async Task<ActionResult<List<ReviewResponseDto>>> GetAllUserReviews()
    {
        var command = new GetUserReviewsCommand
        {
            UserId = GetCurrentUserId()
        };

        var reviewsdto =  await _mediator.Send(command);

        return reviewsdto;
    }

    [HttpGet("{productId}")]
    public async Task<ActionResult<List<ReviewResponseDto>>> GetProductReviews(int productId)
    {
        var command = new GetProductReviewsCommand
        {
            ProductId = productId
        };

        var result = await _mediator.Send(command);

        return result;
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<int>> LeaveComment([FromBody] WebApi.DTOs.Review.AddReviewDto dto)
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


        var command = new AddReviewCommand
        {
            UserId = GetCurrentUserId(),
            ProductId = dto.ProductId,
            Text = dto.Text,
            Rating = dto.Rating,
            Files = dto.Files?.Select(f => new FileUploadDto
            {
                Stream = f.OpenReadStream(),
                FileName = f.FileName,
                ContentType = f.ContentType,
                Length = f.Length,
            }).ToList(),
        };

        return await _mediator.Send(command);
    }

    [Authorize]
    [HttpPut()]
    public async Task<IActionResult> EditReview([FromBody] EditReviewDto dto)
    {
        var command = new EditReviewCommamd
        {
            UserId = GetCurrentUserId(),
            ReviewId = dto.RevieweId,
            NewRating = dto.NewRating,
            NewText = dto.NewText,
        };

        await _mediator.Send(command);

        return Ok();
    }


    [Authorize(Roles = "User")]
    [HttpPost("{productId}/reviews")]
    public async Task<ActionResult<List<FileUploadResponseDto>>> UploadFiles(
    int reviewtId,
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
        var command = new UploadReviewFilesCommand
        {
            ReviewId = reviewtId,
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

    private int GetCurrentUserId()
    {
        var claim = User.FindFirst("userId") ?? User.FindFirst(ClaimTypes.NameIdentifier);
        return int.Parse(claim!.Value);
    }
}
