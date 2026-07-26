using Application.Interfaces;
using Domain.Exceptions;
using MediatR;
using Microsoft.IdentityModel.Tokens.Experimental;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Commands.Review;

public class EditReviewCommamdHandler : IRequestHandler<EditReviewCommamd>
{
    private readonly IReviewRepository _reviewRepository;
    private readonly IUnitOfWork _unitOfWork;

    public EditReviewCommamdHandler(IReviewRepository reviewRepository, IUnitOfWork unitOfWork)
    {
        _reviewRepository = reviewRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(EditReviewCommamd commamd, CancellationToken ct)
    {

        var review = await _reviewRepository.GetReviewById(commamd.ReviewId, ct);

        if (review == null) throw new ArgumentNullException(nameof(review));

        if (review.UserId != commamd.UserId) throw new DomainException("The review don't belong to this USer");

        review.Update(commamd.NewText, commamd.NewRating);

        await _unitOfWork.SaveChangesAsync();

    }
}
