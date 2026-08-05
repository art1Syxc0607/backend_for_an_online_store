using Application.Commands.Product;
using Application.DTOs.File;
using Application.Interfaces;
using Domain.Entities;
using Domain.Exceptions;
using MediatR;
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
    private readonly IUnitOfWork _unitOfWork;

    public UploadReviewFilesHandler(IReviewRepository reviewRepository, 
        IFileStorageService fileStorageService, IUnitOfWork unitOfWork)
    {
        _reviewRepository = reviewRepository;
        _fileStorageService = fileStorageService;
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
            var url = await _fileStorageService.UploadFileAsync(
                file.Stream,
                file.FileName,
                file.ContentType,
                $"reviews/{command.ReviewId}",
                ct
            );

            // Определяем тип и сохраняем в соответствующую коллекцию
            if (file.ContentType.StartsWith("image/"))
                imageUrls.Add(url);
            else if (file.ContentType.StartsWith("video/"))
                videoUrls.Add(url);

            result.Add(new FileUploadResponseDto
            {
                OriginalFileName = file.FileName,
                FileUrl = url,
                ContentType = file.ContentType,
                Size = file.Length
            });
        }

        // Обновляем review
        review.SetImageUrls(imageUrls);
        review.SetVideoUrls(videoUrls);

        await _unitOfWork.SaveChangesAsync(ct);

        return result;
    }


}
