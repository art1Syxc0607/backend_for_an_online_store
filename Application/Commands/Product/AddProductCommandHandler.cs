using Application.DTOs.Product;
using Application.Interfaces;
using Domain.Entities;
using Domain.Exceptions;
using MediatR;
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
    private readonly IMediator _mediator;
    private readonly IUnitOfWork _unitOfWork;

    public AddProductCommandHandler(IProductRepository productRepository, IUnitOfWork unitOfWork,
        ICategoryRepository categoryRepository, IMediator mediator, IFileStorageService fileStorageService)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
        _categoryRepository = categoryRepository;
        _mediator = mediator;
        _fileStorageService = fileStorageService;
    }

    public async Task<int> Handle(AddProductCommand command, CancellationToken ct)
    {
        // 1. Проверка категории
        if (command.CategoryId.HasValue)
        {
            var category = await _categoryRepository.GetByIdAsync(command.CategoryId.Value, ct);
            if (category == null)
                throw new DomainException($"Category {command.CategoryId} not found");
        }

        // 2. Создаем продукт
        var product = new Domain.Entities.Product(
            command.Name,
            command.Price,
            command.StockQuantity,
            command.Description,
            command.CategoryId
        );

        await _productRepository.AddProductAsync(product, ct);

        // 3. Если есть файлы — сохраняем и обновляем продукт
        var imageUrls = new List<string>();
        var videoUrls = new List<string>();

        if (command.Files != null)
        {
            foreach (var file in command.Files)
            {
                // Сохраняем файл
                var url = await _fileStorageService.UploadFileAsync(
                    file.Stream,
                    file.FileName,
                    file.ContentType,
                    $"products/{product.Id}",
                    ct
                );

                // Определяем тип и сохраняем в соответствующую коллекцию
                if (file.ContentType.StartsWith("image/"))
                    imageUrls.Add(url);
                else if (file.ContentType.StartsWith("video/"))
                    videoUrls.Add(url);

            }

            // Обновляем продукт
            product.SetImageUrls(imageUrls);
            product.SetVideoUrls(videoUrls);
        }

        // 4. Единое сохранение!
        await _unitOfWork.SaveChangesAsync(ct);



        return product.Id;
    }
}
