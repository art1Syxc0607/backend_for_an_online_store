using Application.DTOs.Review;
using MediatR;

namespace Application.Commands.Review;

public class GetUserReviewsCommand : IRequest<List<ReviewResponseDto>>
{
    public int UserId { get; set; }
}
