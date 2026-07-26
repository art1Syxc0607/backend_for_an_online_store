using Application.DTOs.Review;
using Application.Interfaces;
using MediatR;
using Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Commands.Review;

public class GetUserReviewsCommandHandler : IRequestHandler<GetUserReviewsCommand, List<ReviewResponseDto>>
{
    private readonly IReviewRepository _reviewRepositiry;

    public GetUserReviewsCommandHandler(IReviewRepository reviewRepositiry) =>
        _reviewRepositiry = reviewRepositiry;

    public async Task<List<ReviewResponseDto>> Handle(GetUserReviewsCommand command, CancellationToken ct)
    {
        var userReviews = await _reviewRepositiry.GetUserReviews(command.UserId, ct);
        if (userReviews == null || userReviews.Count == 0) new DomainException("The User didn't left comments");

        var userReviewsdto = userReviews.Select(r => new ReviewResponseDto
        {
            ProductId = r.ProductId,
            Text = r.Text,
            Rating = r.Rating,
            CreatedAt = r.CreatedAt,
            UpdatedAt = r.UpdatedAt,
            UserId = r.UserId,
        }).ToList();

        return userReviewsdto;
    }

}
