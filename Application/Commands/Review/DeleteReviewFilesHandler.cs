using Application.DTOs.Product;
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

namespace Application.Commands.Review;

public class DeleteReviewFilesHandler : IRequestHandler<DeleteReviewFilesCommand, DeleteFilesResponseDto>
{
    private readonly IReviewRepository _reviewRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<DeleteReviewFilesHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteReviewFilesHandler(IReviewRepository reviewRepository, 
        IFileStorageService fileStorageService, ILogger<DeleteReviewFilesHandler> logger, 
        IUnitOfWork unitOfWork)
    {
        _reviewRepository = reviewRepository;
        _fileStorageService = fileStorageService;
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task<DeleteFilesResponseDto> Handle(DeleteReviewFilesCommand command, CancellationToken ct)
    {
        if (command.FileUrls == null || !command.FileUrls.Any())
            throw new DomainException("No files specified for deletion");

        var review = await _reviewRepository.GetReviewByIdAsync(command.ReviewId, ct);
        if (review == null) throw new DomainException($"Review with ID {command.ReviewId} not found");
        if (review.UserId != command.UserId)
            throw new DomainException($"Review with ID {command.ReviewId} doesn't belong to the user");

        var response = new DeleteFilesResponseDto();
        var urlsToDelete = new List<string>();

        // 1. Сначала меняем только БД-состояние
        foreach (var fileUrl in command.FileUrls.Distinct())
        {
            var isImage = review._imageUrls.Contains(fileUrl);
            var isVideo = !isImage && review._videoUrls.Contains(fileUrl);

            if (!isImage && !isVideo)
            {
                response.NotFoundUrls.Add(fileUrl);
                _logger.LogWarning("FileUrl: {fileUrl}, isn't found.", fileUrl);
                continue;
            }

            if (isImage)
                review.RemoveImage(fileUrl);
            else
                review.RemoveVideo(fileUrl);

            response.DeletedUrls.Add(fileUrl);
            urlsToDelete.Add(fileUrl);
        }

        // 2. Коммитим изменения в БД
        await _unitOfWork.SaveChangesAsync(ct);

        // 3. И только после успешного коммита удаляем физические файлы
        await TryDeleteFilesAsync(urlsToDelete, ct);

        // 4. ✅ Заполняем контекст инвалидации
        // Handler ЗНАЕТ, какие префиксы нужны, но НЕ вызывает кэш напрямую
        command.AddCachePrefix(CacheKeys.ReviewsForProduct(review.ProductId));
        command.AddCachePrefix(CacheKeys.Product(review.ProductId));
        command.AddCachePrefix(CacheKeys.ProductsPrefix);

        return response;
    }

    private async Task TryDeleteFilesAsync(IEnumerable<string> urls, CancellationToken ct)
    {
        foreach (var url in urls)
        {
            try
            {
                await _fileStorageService.DeleteFileAsync(url, ct);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "Failed to delete physical file {FileUrl} after DB commit", url);
            }
        }
    }
}
