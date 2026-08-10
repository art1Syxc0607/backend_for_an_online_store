using Domain.Entities;
using Application.DTOs.Review;
using Application.Commands.Admin.Review;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WebApi.Controllers.Admin;

[ApiController]
[Route("api/admin/reviews")]
[Authorize(Roles = "Admin")]
public class AdminReviewsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminReviewsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    //[HttpGet("{reviewId}")]
    //public async Task<ActionResult<ReviewResponseDto>> GetReviewDetails(int reviewId)
    //{
    //    var query = new GetReviewDetailsQuery
    //    {
    //        ReviewId = reviewId,
    //        AdminId = GetCurrentUserId()
    //    };

    //    var result = await _mediator.Send(query);
    //    return Ok(result);
    //}

    [HttpPost("{reviewId}/respond")]
    public async Task<IActionResult> RespondToReview(int reviewId, [FromBody] RespondToReviewDto dto)
    {
        var command = new RespondToReviewCommand
        {
            ReviewId = reviewId,
            AdminId = GetCurrentUserId(),
            Response = dto.Response
        };

        await _mediator.Send(command);
        return Ok(new { message = "Response added successfully" });
    }

    [HttpPut("{reviewId}/response")]
    public async Task<IActionResult> UpdateReviewResponse(int reviewId, [FromBody] EditReviewDto dto)
    {
        var command = new UpdateReviewResponseCommand
        {
            ReviewId = reviewId,
            NewResponse = dto.NewText
        };

        await _mediator.Send(command);
        return Ok(new { message = "Response updated successfully" });
    }

    [HttpDelete("{reviewId}/response")]
    public async Task<IActionResult> RemoveReviewResponse(int reviewId)
    {
        var command = new RemoveReviewResponseCommand
        {
            ReviewId = reviewId
        };

        await _mediator.Send(command);
        return NoContent();
    }

    private int GetCurrentUserId()
    {
        var claim = User.FindFirst("userId") ?? User.FindFirst(ClaimTypes.NameIdentifier);
        return int.Parse(claim!.Value);
    }
}
