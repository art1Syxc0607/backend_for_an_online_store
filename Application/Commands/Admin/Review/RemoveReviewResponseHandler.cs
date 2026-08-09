using Application.Interfaces;
using Domain.Exceptions;
using MediatR;

namespace Application.Commands.Admin.Review;


public class RemoveReviewResponseHandler : IRequestHandler<RemoveReviewResponseCommand>
{
    private readonly IReviewRepository _reviewRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RemoveReviewResponseHandler(IReviewRepository reviewRepository, IUnitOfWork unitOfWork)
    {
        _reviewRepository = reviewRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(RemoveReviewResponseCommand command, CancellationToken ct)
    {
        var review = await _reviewRepository.GetReviewByIdAsync(command.ReviewId, ct);
        if (review == null)
            throw new DomainException("Review not found");

        if (review.AdminResponse == null)
            throw new DomainException("No response to remove");

        review.RemoveAdminResponse();
        await _unitOfWork.SaveChangesAsync(ct);
    }
}
