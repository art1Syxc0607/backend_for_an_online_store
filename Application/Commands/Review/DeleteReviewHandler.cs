using Application.Interfaces;
using Application.Interfaces.Caching;
using Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Commands.Review;

public class DeleteReviewHandler : IRequestHandler<DeleteReviewCommand>
{
    private readonly IReviewRepository _reviewRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<DeleteReviewHandler> _logger;

    public DeleteReviewHandler(
        IReviewRepository reviewRepository,
        IUnitOfWork unitOfWork,
        IFileStorageService fileStorageService,
        ILogger<DeleteReviewHandler> logger)
    {
        _reviewRepository = reviewRepository;
        _unitOfWork = unitOfWork;
        _fileStorageService = fileStorageService;
        _logger = logger;
    }

    public async Task Handle(DeleteReviewCommand command, CancellationToken ct)
    {
        await _unitOfWork.BeginTransactionAsync(ct);

        try
        {
            // 1. Блокируем строку отзыва, чтобы параллельное редактирование
            //    или удаление не привело к потере данных / двойному удалению.
            var review = await _reviewRepository.GetReviewForUpdateAsync(
                command.ReviewId, ct);

            if (review == null)
            {
                await _unitOfWork.RollbackTransactionAsync(CancellationToken.None);
                throw new DomainException(
                    $"Review with ID {command.ReviewId} not found",
                    "NOT_FOUND",
                    404);
            }

            // 2. Удалять можно только свой отзыв
            if (review.UserId != command.UserId)
            {
                await _unitOfWork.RollbackTransactionAsync(CancellationToken.None);
                throw new DomainException(
                    "You can only delete your own reviews",
                    "FORBIDDEN",
                    403);
            }

            // 3. Запоминаем данные до удаления
            var productId = review.ProductId;
            var fileUrls = review._imageUrls
                .Concat(review._videoUrls)
                .Distinct()
                .ToList();

            // 4. Удаляем из БД
            await _reviewRepository.DeleteReviewAsync(review, ct);
            await _unitOfWork.SaveChangesAsync(ct);
            await _unitOfWork.CommitTransactionAsync(ct);

            // 5. Инвалидируем кэш
            command.AddCachePrefix(CacheKeys.ReviewsForProduct(productId));
            command.AddCachePrefix(CacheKeys.Product(productId));
            command.AddCachePrefix(CacheKeys.ProductsPrefix);

            // 6. Только после успешного коммита удаляем физические файлы.
            //    Ошибка удаления файла не должна откатывать уже удалённый отзыв.
            await TryDeleteFilesAsync(fileUrls, ct);

            _logger.LogInformation(
                "Review {ReviewId} deleted by user {UserId}",
                command.ReviewId,
                command.UserId);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(CancellationToken.None);
            throw;
        }
    }

    private async Task TryDeleteFilesAsync(List<string> urls, CancellationToken ct)
    {
        if (urls.Count == 0)
            return;

        try
        {
            await _fileStorageService.DeleteMultipleFilesAsync(urls, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to delete {Count} file(s) of deleted review",
                urls.Count);
        }
    }
}
