using Application.DTOs.Review;
using Application.Interfaces;
using MediatR;
using Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Queries.Review;

public class GetProductReviewscommandHandler : IRequestHandler<GetProductReviewsCommand, List<ReviewResponseDto>>
{
    private readonly IReviewRepository _reviewRepository;
    private readonly IProductRepository _productRepository;

    public GetProductReviewscommandHandler(IReviewRepository reviewRepository, IProductRepository productRepository)
    {
        _reviewRepository = reviewRepository;
        _productRepository = productRepository;
    }

    public async Task<List<ReviewResponseDto>> Handle(GetProductReviewsCommand query, CancellationToken ct)
    {
        if (!await _productRepository.ProductExist(query.ProductId)) throw new DomainException("No such product");

        var productReviews = await _reviewRepository.GetProductReviews(query.ProductId, ct);

        var reviewsDto = productReviews.Select(r => new ReviewResponseDto
        {
            Id = r.Id,
            UserId = r.UserId,
            ProductId = r.ProductId,
            Text = r.Text,
            Rating = r.Rating,
            CreatedAt = r.CreatedAt,
            UpdatedAt = r.UpdatedAt,
        }).ToList();

        return reviewsDto;
    }
}

