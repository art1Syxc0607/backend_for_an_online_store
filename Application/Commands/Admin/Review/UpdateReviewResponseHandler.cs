using Application.Interfaces;
using Application.Interfaces.Caching;
using Domain.Entities;
using Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Commands.Admin.Review;



public class UpdateReviewResponseHandler : IRequestHandler<UpdateReviewResponseCommand>
{
    private readonly IReviewRepository _reviewRepository;
    private readonly ILogger<UpdateReviewResponseHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateReviewResponseHandler(IReviewRepository reviewRepository,
        ILogger<UpdateReviewResponseHandler> logger,
        IUnitOfWork unitOfWork)
    {
        _reviewRepository = reviewRepository;
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(UpdateReviewResponseCommand command, CancellationToken ct)
    {
        var review = await _reviewRepository.GetReviewByIdAsync(command.ReviewId, ct);
        if (review == null)
            throw new DomainException("Review not found");

        review.UpdateAdminResponse(command.NewResponse);

        await _unitOfWork.SaveChangesAsync(ct);

        // ✅ Указываем префиксы для инвалидации
        command.AddCachePrefix(CacheKeys.ReviewsForProduct(review.ProductId));
        command.AddCachePrefix(CacheKeys.Product(review.ProductId));
        command.AddCachePrefix(CacheKeys.ProductsPrefix);

        _logger.LogInformation(
            "Admin {AdminId} updated response for review {ReviewId}",
            command.AdminId, review.Id);
    }
}