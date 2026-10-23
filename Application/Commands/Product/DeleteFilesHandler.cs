using Application.DTOs.Product;
using Application.Interfaces;
using Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Commands.Product;

// Application/Commands/Product/DeleteFilesHandler.cs
public class DeleteFilesHandler : IRequestHandler<DeleteFilesCommand, DeleteFilesResponseDto>
{
    private readonly IProductRepository _productRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<DeleteFilesHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteFilesHandler(
        IProductRepository productRepository,
        IFileStorageService fileStorageService,
        ILogger<DeleteFilesHandler> logger,
        IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _fileStorageService = fileStorageService;
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task<DeleteFilesResponseDto> Handle(DeleteFilesCommand command, CancellationToken ct)
    {
        if (command.FileUrls == null || !command.FileUrls.Any())
            throw new DomainException("No files specified for deletion");

        // 1. Проверяем продукт
        var product = await _productRepository.GetByIdAsync(command.ProductId, ct);
        if (product == null)
        {
            _logger.LogWarning("Product with ID {ProductId} not found",
                command.ProductId);

            throw new DomainException($"Product with ID {command.ProductId} not found");
        }

        var response = new DeleteFilesResponseDto();
        var urlsToDelete = new List<string>();

        // 2. Сначала меняем только БД-состояние. Физические файлы удаляем
        //    только после успешного SaveChanges, иначе при ошибке БД
        //    товар останется со ссылками на уже удалённые файлы.
        foreach (var fileUrl in command.FileUrls.Distinct())
        {
            var isImage = product.ImageUrls.Contains(fileUrl);
            var isVideo = !isImage && product.VideoUrls.Contains(fileUrl);

            if (!isImage && !isVideo)
            {
                response.NotFoundUrls.Add(fileUrl);
                _logger.LogWarning("FileUrl: {fileUrl}, isn't found.", fileUrl);
                continue;
            }

            if (isImage)
                product.RemoveImage(fileUrl);
            else
                product.RemoveVideo(fileUrl);

            response.DeletedUrls.Add(fileUrl);
            urlsToDelete.Add(fileUrl);
        }

        // 3. Коммитим изменения в БД
        await _unitOfWork.SaveChangesAsync(ct);

        // 4. После успешного коммита удаляем физические файлы.
        //    Ошибка удаления файла больше не ломает состояние БД:
        //    останется только orphan-файл, который можно убрать отдельно.
        await TryDeleteFilesAsync(urlsToDelete, ct);

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
