using Application.Commands.Product;
using Application.DTOs.File;
using Application.Interfaces;
using Application.Interfaces.Caching;
using Application.Validators.Common;
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

public class UploadReviewFilesHandler : IRequestHandler<UploadReviewFilesCommand, List<FileUploadResponseDto>>
{
    private readonly IReviewRepository _reviewRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<UploadReviewFilesHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public UploadReviewFilesHandler(IReviewRepository reviewRepository, 
        IFileStorageService fileStorageService, ILogger<UploadReviewFilesHandler> logger,
        IUnitOfWork unitOfWork)
    {
        _reviewRepository = reviewRepository;
        _fileStorageService = fileStorageService;
        _logger = logger;
        _unitOfWork = unitOfWork;
    }


    public async Task<List<FileUploadResponseDto>> Handle(UploadReviewFilesCommand command, CancellationToken ct)
    {
        var review = await _reviewRepository.GetReviewByIdAsync(command.ReviewId, ct);
        if (review == null)
            throw new DomainException($"Review with ID {command.ReviewId} not found");

        // ✅ Проверка владельца: пользователь может загружать файлы только в свой отзыв
        if (review.UserId != command.UserId)
            throw new DomainException($"Review with ID {command.ReviewId} doesn't belong to the user");

        if (command.Files == null || command.Files.Count == 0)
            throw new DomainException("No files specified for upload");

        foreach (var file in command.Files)
        {
            if (!AllowedFileTypes.IsAllowed(file.ContentType))
                throw new DomainException($"Unsupported file type: {file.ContentType}");
        }

        // ✅ Валидация ДО записи на диск — чтобы не оставить orphan-файлы
        review.EnsureCanAddFiles(command.Files.Count);

        var result = new List<FileUploadResponseDto>();
        var imageUrls = new List<string>();
        var videoUrls = new List<string>();
        var uploadedUrls = new List<string>();

        try
        {
            foreach (var file in command.Files)
            {
                var url = await _fileStorageService.UploadFileAsync(
                    file.Stream,
                    file.FileName,
                    file.ContentType,
                    $"reviews/{review.Id}",
                    ct);
                uploadedUrls.Add(url);

                result.Add(new FileUploadResponseDto
                {
                    OriginalFileName = file.FileName,
                    FileUrl = url,
                    ContentType = file.ContentType,
                    Size = file.Length
                });

                if (file.ContentType.StartsWith("image/"))
                    imageUrls.Add(url);
                else if (file.ContentType.StartsWith("video/"))
                    videoUrls.Add(url);
            }

            // Обновляем review
            if (imageUrls.Any())
                review.SetImageUrls(imageUrls);
            if (videoUrls.Any())
                review.SetVideoUrls(videoUrls);

            await _unitOfWork.SaveChangesAsync(ct);

            // ✅ Указываем префиксы для инвалидации
            command.AddCachePrefix(CacheKeys.ReviewsForProduct(review.ProductId));
            command.AddCachePrefix(CacheKeys.Product(review.ProductId));
            command.AddCachePrefix(CacheKeys.ProductsPrefix);

            return result;
        }
        catch
        {
            // ✅ Компенсация: БД откатит SaveChanges, а файлы удаляем вручную
            await TryDeleteUploadedFilesAsync(uploadedUrls, ct);
            throw;
        }
    }

    private async Task TryDeleteUploadedFilesAsync(List<string> urls, CancellationToken ct)
    {
        if (urls.Count == 0)
            return;

        try
        {
            await _fileStorageService.DeleteMultipleFilesAsync(urls, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to roll back {Count} uploaded review file(s)", urls.Count);
        }
    }
}
