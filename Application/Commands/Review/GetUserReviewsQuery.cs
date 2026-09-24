using Application.DTOs.Review;
using MediatR;

namespace Application.Commands.Review;

public class GetUserReviewsQuery : IRequest<List<ReviewResponseDto>>
{
    public int UserId { get; set; }
}
