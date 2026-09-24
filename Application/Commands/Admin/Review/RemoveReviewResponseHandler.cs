using Application.Interfaces;
using Application.Interfaces.Caching;
using Domain.Entities;
using Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Commands.Admin.Review;


public class RemoveReviewResponseHandler : IRequestHandler<RemoveReviewResponseCommand>
{
    private readonly IReviewRepository _reviewRepository;
    private readonly ILogger<RemoveReviewResponseHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public RemoveReviewResponseHandler(IReviewRepository reviewRepository,
        ILogger<RemoveReviewResponseHandler> logger,
        IUnitOfWork unitOfWork)
    {
        _reviewRepository = reviewRepository;
        _logger = logger;
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

        // 6. ✅ Указываем префиксы для инвалидации
        command.AddCachePrefix(CacheKeys.ReviewsForProduct(review.ProductId));
        command.AddCachePrefix(CacheKeys.Product(review.ProductId));
        command.AddCachePrefix(CacheKeys.ProductsPrefix);

        _logger.LogInformation(
           "Admin {AdminId} removed response from review {ReviewId}",
           command.AdminId, review.Id);

    }
}
