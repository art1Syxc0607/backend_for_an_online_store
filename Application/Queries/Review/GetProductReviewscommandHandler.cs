using Application.DTOs.Review;
using Application.Interfaces;
using Application.Common;
using MediatR;
using Domain.Exceptions;
using Domain.Entities;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Queries.Review;

public class GetProductReviewscommandHandler : IRequestHandler<GetProductReviewsQuery, List<ReviewResponseDto>>
{
    private readonly IReviewRepository _reviewRepository;
    private readonly IMapper _mapper;
    private readonly IProductRepository _productRepository;

    public GetProductReviewscommandHandler(IReviewRepository reviewRepository,
        IMapper mapper,
        IProductRepository productRepository)
    {
        _reviewRepository = reviewRepository;
        _mapper = mapper;
        _productRepository = productRepository;
    }

    public async Task<List<ReviewResponseDto>> Handle(GetProductReviewsQuery query, CancellationToken ct)
    {
        if (!await _productRepository.ProductExist(query.ProductId)) throw new DomainException("No such product");

        var productReviews = await _reviewRepository.GetProductReviewsAsync(query, ct);

        var reviewsDto = _mapper.Map<List<ReviewResponseDto>>(productReviews);

        return reviewsDto;
    }
}

