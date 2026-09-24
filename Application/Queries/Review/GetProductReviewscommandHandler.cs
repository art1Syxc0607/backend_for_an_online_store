using Application.DTOs.Review;
using Application.Interfaces;
using Application.Common;
using Application.Common.Extensions;
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

public class GetProductReviewscommandHandler : IRequestHandler<GetProductReviewsQuery, PagedResult<ReviewResponseDto>>
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

    public async Task<PagedResult<ReviewResponseDto>> Handle(GetProductReviewsQuery query, CancellationToken ct)
    {
        if (!await _productRepository.ProductExist(query.ProductId)) throw new DomainException("No such product");

        // 1. Репозиторий возвращает PagedResult<Review>
        var pagedReviews = await _reviewRepository
            .GetProductReviewsAsync(query, ct);

        // 2. ✅ Одна строка — маппинг
        return pagedReviews.Map<Domain.Entities.Review, ReviewResponseDto>(_mapper);
    }
}

