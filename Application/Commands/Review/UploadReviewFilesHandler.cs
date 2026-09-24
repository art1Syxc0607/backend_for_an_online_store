using Application.Commands.Product;
using Application.DTOs.File;
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

        var result = new List<FileUploadResponseDto>();
        var imageUrls = new List<string>();
        var videoUrls = new List<string>();

        foreach (var file in command.Files)
        {
            // Сохраняем файл
            try
            {
                var url = await _fileStorageService.UploadFileAsync(
                    file.Stream,
                    file.FileName,
                    file.ContentType,
                    $"reviews/{review.Id}",
                    ct);

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
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to upload file {FileName} for review {ReviewId}",
                    file.FileName, review.Id);
                // Продолжаем с другими файлами
            }
        }

        // Обновляем review
        review.SetImageUrls(imageUrls);
        review.SetVideoUrls(videoUrls);

        await _unitOfWork.SaveChangesAsync(ct);

        // ✅ Указываем префиксы для инвалидации
        command.AddCachePrefix(CacheKeys.ReviewsForProduct(review.ProductId));
        command.AddCachePrefix(CacheKeys.Product(review.ProductId));
        command.AddCachePrefix(CacheKeys.ProductsPrefix);

        return result;
    }


}
