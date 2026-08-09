using MediatR;

namespace Application.Commands.Admin.Review;

public class RemoveReviewResponseCommand : IRequest
{
    public int ReviewId { get; init; }
}