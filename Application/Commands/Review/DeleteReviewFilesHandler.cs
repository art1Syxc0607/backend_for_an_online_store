using Application.DTOs.Product;
using Application.Interfaces;
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
        if(review == null) throw new DomainException($"Review with ID {command.ReviewId} not found");
        if (review.UserId != command.UserId)
            throw new DomainException($"Review with ID {command.ReviewId} doesn't belong to the user");


        var response = new DeleteFilesResponseDto();

        foreach (var fileUrl in command.FileUrls)
        {
            // 2. Проверяем, есть ли такой URL у продукта
            var isImage = review.ImageUrls.Contains(fileUrl);
            var isVideo = review.VideoUrls.Contains(fileUrl);

            if (!isImage && !isVideo)
            {
                response.NotFoundUrls.Add(fileUrl);
                _logger.LogWarning("FileUrl: {fileUrl}, isn't found.", fileUrl);
                continue;
            }

            try
            {
                // 3. Удаляем физический файл
                await _fileStorageService.DeleteFileAsync(fileUrl, ct);

                // 4. Удаляем URL из сущности
                if (isImage)
                    review.RemoveImage(fileUrl);
                else if (isVideo)
                    review.RemoveVideo(fileUrl);

                response.DeletedUrls.Add(fileUrl);
            }
            catch (Exception ex)
            {
                response.FailedUrls.Add(fileUrl);
                // Логируем ошибку, но продолжаем удалять другие файлы
                _logger.LogWarning("Error to delete fileUrl: {fileUrl}.", fileUrl);
            }
        }

        // 5. Сохраняем изменения
        await _unitOfWork.SaveChangesAsync(ct);

        return response;


    }
}
