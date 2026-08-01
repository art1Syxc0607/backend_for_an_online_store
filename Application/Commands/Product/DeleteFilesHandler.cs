using Application.DTOs.Product;
using Application.Interfaces;
using Domain.Exceptions;
using MediatR;
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
    private readonly IUnitOfWork _unitOfWork;

    public DeleteFilesHandler(
        IProductRepository productRepository,
        IFileStorageService fileStorageService,
        IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _fileStorageService = fileStorageService;
        _unitOfWork = unitOfWork;
    }

    public async Task<DeleteFilesResponseDto> Handle(DeleteFilesCommand command, CancellationToken ct)
    {
        if (command.FileUrls == null || !command.FileUrls.Any())
            throw new DomainException("No files specified for deletion");

        // 1. Проверяем продукт
        var product = await _productRepository.GetByIdAsync(command.ProductId, ct);
        if (product == null)
            throw new DomainException($"Product with ID {command.ProductId} not found");

        var response = new DeleteFilesResponseDto();

        foreach (var fileUrl in command.FileUrls)
        {
            // 2. Проверяем, есть ли такой URL у продукта
            var isImage = product.ImageUrls.Contains(fileUrl);
            var isVideo = product.VideoUrls.Contains(fileUrl);

            if (!isImage && !isVideo)
            {
                response.NotFoundUrls.Add(fileUrl);
                continue;
            }

            try
            {
                // 3. Удаляем физический файл
                await _fileStorageService.DeleteFileAsync(fileUrl, ct);

                // 4. Удаляем URL из сущности
                if (isImage)
                    product.RemoveImage(fileUrl);
                else if (isVideo)
                    product.RemoveVideo(fileUrl);

                response.DeletedUrls.Add(fileUrl);
            }
            catch (Exception ex)
            {
                response.FailedUrls.Add(fileUrl);
                // Логируем ошибку, но продолжаем удалять другие файлы
            }
        }

        // 5. Сохраняем изменения
        await _unitOfWork.SaveChangesAsync(ct);

        return response;
    }
}
