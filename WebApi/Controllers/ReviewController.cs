using Application.Commands.Review;
using Application.Commands.User;
using Application.DTOs.Review;
using Application.DTOs.User;
using Application.Queries.Review;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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
    public async Task<ActionResult<int>> LeaveComment([FromBody] AddReviewDto dto)
    {
        var command = new AddReviewCommand
        {
            UserId = GetCurrentUserId(),
            ProductId = dto.ProductId,
            Text = dto.Text,
            Rating = dto.Rating,
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

    private int GetCurrentUserId()
    {
        var claim = User.FindFirst("userId") ?? User.FindFirst(ClaimTypes.NameIdentifier);
        return int.Parse(claim!.Value);
    }
}
