using Application.DTOs.Review;
using Application.Interfaces;
using Application.Queries.Review;
using MediatR;
using Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Application.Queries.Admin.Review;

public class GetReviewByIdHandler : IRequestHandler<GetReviewByIdCommand, ReviewResponseDto>
{
    private readonly IReviewRepository _reviewRepository;
    private readonly ILogger<GetReviewByIdHandler> _logger;

    public GetReviewByIdHandler(IReviewRepository reviewRepository,
        ILogger<GetReviewByIdHandler> logger)
    {
        _reviewRepository = reviewRepository;
        _logger = logger;
    }

    public async Task<ReviewResponseDto> Handle(GetReviewByIdCommand query, CancellationToken ct)
    {
        var review = await _reviewRepository.GetReviewByIdAsync(query.Id, ct);
        if (review == null) 
        {
            _logger.LogWarning(
                "Review with Id - {query.Id} isn't found",
                query.Id
            );

            throw new DomainException($"There's no a review with Id: {query.Id}");      
        }

        var result = new ReviewResponseDto
        {

            Id = review.Id,
            UserId = review.UserId,
            UserName = review.User?.UserName ?? "Unknown", // ← Теперь есть!
            ProductId = review.ProductId,
            Text = review.Text,
            Rating = review.Rating,
            Status = review.Status,
            CreatedAt = review.CreatedAt,
            UpdatedAt = review.UpdatedAt,
            AdminResponse = review.AdminResponse,
            AdminResponseAt = review.AdminResponseAt
        };

        return result;
    }
}
