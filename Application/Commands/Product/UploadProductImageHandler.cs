using Application.Interfaces;
using Domain.Exceptions;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Commands.Product;

public class UploadProductImageHandler : IRequestHandler<UploadProductImageCommand, string>
{
    private readonly IProductRepository _productRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly IUnitOfWork _unitOfWork;

    public UploadProductImageHandler(
        IProductRepository productRepository,
        IFileStorageService fileStorageService,
        IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _fileStorageService = fileStorageService;
        _unitOfWork = unitOfWork;
    }

    public async Task<string> Handle(UploadProductImageCommand command, CancellationToken ct)
    {
        // 1. Проверяем, существует ли товар
        var product = await _productRepository.GetByIdAsync(command.ProductId, ct);
        if (product == null)
            throw new DomainException($"Product with ID {command.ProductId} not found");

        // 2. Сохраняем файл через инфраструктурный сервис
        var imageUrl = await _fileStorageService.UploadFileAsync(
            command.FileStream,
            command.FileName,
            command.ContentType,
            ct
        );

        // 3. Обновляем сущность
        product.SetImageUrl(imageUrl); // ← метод в Domain

        await _unitOfWork.SaveChangesAsync(ct);

        return imageUrl;
    }
}
