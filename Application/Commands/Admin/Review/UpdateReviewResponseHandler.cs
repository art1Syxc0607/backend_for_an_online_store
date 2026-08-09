using Application.Interfaces;
using Domain.Exceptions;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Commands.Admin.Review;



public class UpdateReviewResponseHandler : IRequestHandler<UpdateReviewResponseCommand>
{
    private readonly IReviewRepository _reviewRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateReviewResponseHandler(IReviewRepository reviewRepository, IUnitOfWork unitOfWork)
    {
        _reviewRepository = reviewRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(UpdateReviewResponseCommand command, CancellationToken ct)
    {
        var review = await _reviewRepository.GetReviewByIdAsync(command.ReviewId, ct);
        if (review == null)
            throw new DomainException("Review not found");

        review.UpdateAdminResponse(command.NewResponse);
        await _unitOfWork.SaveChangesAsync(ct);
    }
}