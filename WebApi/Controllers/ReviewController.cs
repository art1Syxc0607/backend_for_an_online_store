using Application.Commands.Product;
using Application.Commands.Review;
using Application.Commands.User;
using Application.Common;
using Application.DTOs.File;
using Application.DTOs.Product;
using Application.DTOs.Review;
using Application.Queries.Review;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebApi.Common;
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
        var command = new GetUserReviewsQuery
        {
            UserId = GetCurrentUserId()
        };

        var reviewsdto =  await _mediator.Send(command);

        return reviewsdto;
    }

    [HttpGet("{productId}")]
    public async Task<ActionResult<PagedResult<ReviewResponseDto>>> GetProductReviews(
        int productId,
        [FromQuery] GetProductReviewsDto dto,
        CancellationToken ct)
    {
        var query = new GetProductReviewsQuery
        {
            ProductId = productId,
            Rating = dto.Rating,
            MinRating = dto.MinRating,
            HasMedia = dto.HasMedia,
            IsVerifiedPurchase = dto.IsVerifiedPurchase,
            HasAdminResponse = dto.HasAdminResponse,
            FromDate = dto.FromDate,
            ToDate = dto.ToDate,
            SortBy = dto.SortBy,
            Descending = dto.Descending,
            PageNumber = dto.PageNumber,
            PageSize = dto.PageSize
        };

        var result = await _mediator.Send(query);

        return result;
    }

    [Authorize]
    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<int>> LeaveComment([FromForm] AddReviewDto dto)
    {
        var validationError = FileUploadValidation.Validate(dto.Files);
        if (validationError != null)
            return BadRequest(validationError);


        var command = new AddReviewCommand
        {
            UserId = GetCurrentUserId(),
            ProductId = dto.ProductId,
            Text = dto.Text,
            Rating = dto.Rating,
            Files = dto.Files?.Select(f => new Application.DTOs.File.FileUploadDto
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
            ReviewId = dto.ReviewId,
            NewRating = dto.NewRating,
            NewText = dto.NewText,
        };

        await _mediator.Send(command);

        return Ok();
    }


    [Authorize(Roles = "User")]
    [HttpPost("{reviewId}/files")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<List<Application.DTOs.File.FileUploadResponseDto>>> UploadFiles(
    int reviewId,
    [FromForm] List<IFormFile> files)
    {
        var validationError = FileUploadValidation.Validate(files, required: true);
        if (validationError != null)
            return BadRequest(validationError);

        var command = new UploadReviewFilesCommand
        {
            ReviewId = reviewId,
            UserId = GetCurrentUserId(),
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

    [Authorize(Roles = "User")]
    [HttpDelete("{reviewId}/files")]
    public async Task<DeleteFilesResponseDto> DeleteReviewFiles([FromBody] List<string> urls, int reviewId)
    {
        var command = new DeleteReviewFilesCommand
        {
            ReviewId = reviewId,
            UserId = GetCurrentUserId(),
            FileUrls = urls
        };

        return await _mediator.Send(command);
    }

    private int GetCurrentUserId()
    {
        var claim = User.FindFirst("userId") ?? User.FindFirst(ClaimTypes.NameIdentifier);
        return int.Parse(claim!.Value);
    }
}
