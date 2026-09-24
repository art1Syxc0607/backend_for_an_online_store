// Application/Commands/Review/EditReviewCommandHandler.cs
using Application.Common.Caching;
using Application.Interfaces;
using Application.Interfaces.Caching;
using Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Commands.Review;

public class EditReviewCommandHandler
    : IRequestHandler<EditReviewCommamd>
{
    private readonly IReviewRepository _reviewRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<EditReviewCommandHandler> _logger;

    public EditReviewCommandHandler(
        IReviewRepository reviewRepository,
        IUnitOfWork unitOfWork,
        ILogger<EditReviewCommandHandler> logger)
    {
        _reviewRepository = reviewRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(
        EditReviewCommamd command,
        CancellationToken ct)
    {
        // 1. Загружаем Review
        var review = await _reviewRepository.GetReviewByIdAsync(
            command.ReviewId, ct);

        if (review == null)
            throw new DomainException("Review not found");

        // 2. Проверяем владельца
        if (review.UserId != command.UserId)
            throw new UnauthorizedAccessException(
                "You can only edit your own reviews");

        // 3. Сохраняем ProductId ДО изменений
        var productId = review.ProductId;

        // 4. Обновляем (только если есть изменения)
        var hasChanges = false;

        if (!string.IsNullOrWhiteSpace(command.NewText)
            && command.NewText != review.Text)
        {
            review.Update(newText: command.NewText);
            hasChanges = true;
        }

        if (command.NewRating.HasValue
            && command.NewRating != review.Rating)
        {
            if (command.NewRating < 1 || command.NewRating > 5)
                throw new DomainException("Rating must be 1-5");

            review.Update(newRating: command.NewRating.Value);
            hasChanges = true;
        }

        if (!hasChanges)
        {
            _logger.LogInformation(
                "No changes for review {ReviewId}", review.Id);
            return;
        }

        // 5. Сохраняем
        await _unitOfWork.SaveChangesAsync(ct);

        // 6. ✅ Указываем префиксы для инвалидации
        command.AddCachePrefix(CacheKeys.ReviewsForProduct(productId));
        command.AddCachePrefix(CacheKeys.Product(productId));
        command.AddCachePrefix(CacheKeys.ProductsPrefix);

        _logger.LogInformation(
            "Review {ReviewId} edited successfully", review.Id);
    }
}