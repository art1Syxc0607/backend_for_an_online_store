using Application.DTOs.Review;
using Application.Interfaces;
using MediatR;
using Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;

namespace Application.Commands.Review;

public class GetUserReviewsCommandHandler : IRequestHandler<GetUserReviewsQuery, List<ReviewResponseDto>>
{
    private readonly IReviewRepository _reviewRepositiry;
    private readonly IMapper _mapper;

    public GetUserReviewsCommandHandler(IReviewRepository reviewRepositiry, IMapper mapper)
    {
        _reviewRepositiry = reviewRepositiry;
        _mapper = mapper;

    }

    public async Task<List<ReviewResponseDto>> Handle(GetUserReviewsQuery command, CancellationToken ct)
    {
        var userReviews = await _reviewRepositiry.GetUserReviews(command.UserId, ct);
        if (userReviews == null || userReviews.Count == 0) new DomainException("The User didn't left comments");

        var userReviewsdto = _mapper.Map<List<ReviewResponseDto>>(userReviews);

        return userReviewsdto;
    }

}
