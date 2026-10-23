using Application.DTOs.File;
using Application.Interfaces;
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

namespace Application.Commands.Product;

public class AddProductCommandHandler : IRequestHandler<AddProductCommand, int>
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<AddProductCommandHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public AddProductCommandHandler(IProductRepository productRepository, 
        ICategoryRepository categoryRepository, 
        IFileStorageService fileStorageService, ILogger<AddProductCommandHandler> logger,
        IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
        _categoryRepository = categoryRepository;
        _logger = logger;
        _fileStorageService = fileStorageService;
    }

    public async Task<int> Handle(AddProductCommand command, CancellationToken ct)
    {
        _logger.LogInformation(
            "Product creation started: Name {Name}, Price {Price}, Stock {Stock}",
            command.Name,
            command.Price,
            command.StockQuantity
        );

        // 1. Проверка категории
        if (command.CategoryId.HasValue)
        {
            var category = await _categoryRepository.GetByIdAsync(command.CategoryId.Value, ct);
            if (category == null)
                throw new DomainException($"Category {command.CategoryId} not found");
        }

        // 2. Валидация ДО записи в БД и на диск — чтобы не создавать товар
        //    и не оставлять orphan-файлы при некорректном запросе.
        var files = command.Files ?? new List<FileUploadDto>();

        foreach (var file in files)
        {
            if (!AllowedFileTypes.IsAllowed(file.ContentType))
                throw new DomainException($"Unsupported file type: {file.ContentType}");
        }

        // 3. Создаем продукт
        var product = new Domain.Entities.Product(
            command.Name,
            command.Price,
            command.PurchasePrice,
            command.StockQuantity,
            command.Description,
            command.CategoryId
        );

        product.EnsureCanAddFiles(files.Count);

        var uploadedUrls = new List<string>();
        var productPersisted = false;

        try
        {
            // 4. Сохраняем товар, чтобы получить реальный Id.
            //    До SaveChanges EF не генерирует идентификатор, поэтому
            //    использовать product.Id в пути загрузки нельзя.
            await _productRepository.AddProductAsync(product, ct);
            await _unitOfWork.SaveChangesAsync(ct);
            productPersisted = true;

            // 5. Если есть файлы — загружаем их в папку уже существующего товара
            if (files.Count > 0)
            {
                var imageUrls = new List<string>();
                var videoUrls = new List<string>();

                foreach (var file in files)
                {
                    var url = await _fileStorageService.UploadFileAsync(
                        file.Stream,
                        file.FileName,
                        file.ContentType,
                        $"products/{product.Id}",
                        ct
                    );
                    uploadedUrls.Add(url);

                    if (file.ContentType.StartsWith("image/"))
                        imageUrls.Add(url);
                    else if (file.ContentType.StartsWith("video/"))
                        videoUrls.Add(url);
                }

                if (imageUrls.Any())
                    product.SetImageUrls(imageUrls);

                if (videoUrls.Any())
                    product.SetVideoUrls(videoUrls);

                // 6. Сохраняем URL медиа у товара
                await _unitOfWork.SaveChangesAsync(ct);
            }

            _logger.LogInformation(
                "Product created successfully: ProductId {ProductId}, Name {Name}, Price {Price}, " +
                "Stock {Stock}",
                product.Id,
                product.Name,
                product.Price,
                product.StockQuantity
            );

            return product.Id;
        }
        catch
        {
            // 7. Компенсация. Сначала откатываем запись товара в БД,
            //    затем удаляем уже загруженные физические файлы.
            if (productPersisted)
                await TryDeleteProductAsync(product, ct);

            await TryDeleteUploadedFilesAsync(uploadedUrls, ct);
            throw;
        }
    }

    private async Task TryDeleteProductAsync(Domain.Entities.Product product, CancellationToken ct)
    {
        try
        {
            await _productRepository.DeleteProductAsync(product, ct);
            await _unitOfWork.SaveChangesAsync(ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to roll back product {ProductId} after media upload error",
                product.Id);
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
