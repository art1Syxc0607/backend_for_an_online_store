using Application.DTOs.File;
using Application.Interfaces;
using Domain.Exceptions;
using MediatR;
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

    public UploadProductFilesHandler(IProductRepository productRepository, IFileStorageService fileStorageService, IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _fileStorageService = fileStorageService;
        _unitOfWork = unitOfWork;
    }

    public async Task<List<FileUploadResponseDto>> Handle(UploadProductFilesCommand command, CancellationToken ct)
    {
        var product = await _productRepository.GetByIdAsync(command.ProductId, ct);
        if (product == null)
            throw new DomainException($"Product with ID {command.ProductId} not found");

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
                $"products/{command.ProductId}",
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

        // Обновляем продукт
        product.SetImageUrls(imageUrls);
        product.SetVideoUrls(videoUrls);

        await _unitOfWork.SaveChangesAsync(ct);

        return result;
    }
}