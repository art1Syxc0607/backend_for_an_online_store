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

        // 3. Если есть фото — сохраняем и обновляем продукт
        if (command.ImageStream != null && !string.IsNullOrEmpty(command.ImageFileName))
        {
            var imageUrl = await _fileStorageService.UploadFileAsync(
                command.ImageStream,
                command.ImageFileName,
                command.ImageContentType ?? "image/jpeg",
                ct
            );
            product.SetImageUrl(imageUrl);
        }

        // 4. Единое сохранение!
        await _unitOfWork.SaveChangesAsync(ct);

        return product.Id;
    }
}
