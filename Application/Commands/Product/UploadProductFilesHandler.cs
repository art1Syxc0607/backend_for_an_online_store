using Application.DTOs.File;
using Application.Interfaces;
using Application.Validators.Common;
using Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Commands.Product;

public class UploadProductFilesHandler : IRequestHandler<UploadProductFilesCommand,
            List<FileUploadResponseDto>>
{
    private readonly IProductRepository _productRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UploadProductFilesHandler> _logger;

    public UploadProductFilesHandler(IProductRepository productRepository,
        IFileStorageService fileStorageService,
        IUnitOfWork unitOfWork,
        ILogger<UploadProductFilesHandler> logger)
    {
        _productRepository = productRepository;
        _fileStorageService = fileStorageService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<List<FileUploadResponseDto>> Handle(UploadProductFilesCommand command, CancellationToken ct)
    {
        var product = await _productRepository.GetByIdAsync(command.ProductId, ct);
        if (product == null)
            throw new DomainException($"Product with ID {command.ProductId} not found");

        if (command.Files == null || command.Files.Count == 0)
            throw new DomainException("No files specified for upload");

        foreach (var file in command.Files)
        {
            if (!AllowedFileTypes.IsAllowed(file.ContentType))
                throw new DomainException($"Unsupported file type: {file.ContentType}");
        }

        // ✅ Валидация ДО записи на диск — чтобы не оставить orphan-файлы
        product.EnsureCanAddFiles(command.Files.Count);

        var result = new List<FileUploadResponseDto>();
        var imageUrls = new List<string>();
        var videoUrls = new List<string>();
        var uploadedUrls = new List<string>();

        try
        {
            foreach (var file in command.Files)
            {
                // Сохраняем файл
                var url = await _fileStorageService.UploadFileAsync(
                    file.Stream,
                    file.FileName,
                    file.ContentType,
                    $"products/{command.ProductId}",
                    ct
                );
                uploadedUrls.Add(url);

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

            // Обновляем продукт
            if (imageUrls.Any())
                product.SetImageUrls(imageUrls);
            if (videoUrls.Any())
                product.SetVideoUrls(videoUrls);

            await _unitOfWork.SaveChangesAsync(ct);

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
            // best-effort: файлы могли не удалиться, но основной сбой важнее
            _logger.LogError(ex,
                "Failed to roll back {Count} uploaded product file(s)", urls.Count);
        }
    }
}
