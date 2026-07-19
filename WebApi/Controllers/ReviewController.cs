using Application.Commands.Review;
using Application.Commands.User;
using Application.DTOs.Review;
using Application.DTOs.User;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WebApi.Controllers;

[Authorize]
[Route("api/review")]
public class ReviewController : Controller
{
    private readonly IMediator _mediator;

    public ReviewController(IMediator mediator)
        => _mediator = mediator;

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

    [HttpPut("{reviewId}")]
    public async Task<IActionResult> EditReview(int reviewId)
    {

    }

    private int GetCurrentUserId()
    {
        var claim = User.FindFirst("userId") ?? User.FindFirst(ClaimTypes.NameIdentifier);
        return int.Parse(claim!.Value);
    }
}
