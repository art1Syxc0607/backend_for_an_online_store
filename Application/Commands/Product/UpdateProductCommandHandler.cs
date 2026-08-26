using Application.Interfaces;
using Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Commands.Product;

public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand>
{
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AddProductCommandHandler> _logger;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ICacheService _cacheService;

    public UpdateProductCommandHandler(IProductRepository productRepository, IUnitOfWork unitOfWork,
        ICategoryRepository cartRepository, ILogger<AddProductCommandHandler> logger, ICacheService cacheService)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
        _categoryRepository = cartRepository;
        _logger = logger;
        _cacheService = cacheService;
    }


    public async Task Handle(UpdateProductCommand command, CancellationToken ct)
    {
        var product = await _productRepository.GetByIdAsync(command.ProductId, ct);
        if (product == null)
        {
            _logger.LogWarning(
                "Update product failed: Product not found. ProductId {ProductId}",
                command.ProductId
            );
            throw new DomainException("Product not found");
        }

        _logger.LogInformation(
            "Update product started: ProductId {ProductId}, Name {Name}",
            command.ProductId,
            product.Name
        );

        // Проверяем категорию
        if (command.CategoryId.HasValue)
        {
            var category = await _categoryRepository.GetByIdAsync(command.CategoryId.Value, ct);
            if (category == null)
            {
                _logger.LogWarning(
                    "Update product failed: Category not found. ProductId {ProductId}, " +
                    "CategoryId {CategoryId}",
                    command.ProductId,
                    command.CategoryId.Value
                );
                throw new DomainException($"Category {command.CategoryId} not found");
            }
            product.AssignCategory(category);
        }

        // Сохраняем старые значения для логирования
        var oldName = product.Name;
        var oldPrice = product.Price;
        var oldPurchasePrice = product.PurchasePrice;
        var oldStockQuantity = product.StockQuantity;
        var oldDescription = product.Description;
        var oldSku = product.Sku;
        var oldCategoryId = product.CategoryId;

        // Обновляем остальные поля
        product.UpdateDetails(
            name: command.Name,
            price: command.Price,
            description: command.Description,
            StockQuantity: command.StockQuantity,
            sku: command.Sku,
            purchasePrice: command.PurchasePrice
        );


        await _productRepository.UpdateProductAsync(product, ct);
        await _unitOfWork.SaveChangesAsync();

        // 6. ✅ Логируем ВСЕ изменения (только если параметр не null)
        var changes = new List<string>();

        if (command.Name != null && oldName != command.Name)
            changes.Add($"Name: '{oldName}' → '{command.Name}'");

        if (command.Price.HasValue && oldPrice != command.Price.Value)
            changes.Add($"Price: {oldPrice:C} → {command.Price.Value:C}");

        if (command.PurchasePrice.HasValue && oldPurchasePrice != command.PurchasePrice.Value)
            changes.Add($"PurchasePrice: {oldPurchasePrice:C} → {command.PurchasePrice.Value:C}");

        if (command.StockQuantity.HasValue && oldStockQuantity != command.StockQuantity.Value)
            changes.Add($"Stock: {oldStockQuantity} → {command.StockQuantity.Value}");

        if (command.Description != null && oldDescription != command.Description)
            changes.Add($"Description updated (length: {oldDescription?.Length ?? 0} → {command.Description.Length})");

        if (command.Sku != null && oldSku != command.Sku)
            changes.Add($"SKU: '{oldSku}' → '{command.Sku}'");

        if (command.CategoryId.HasValue && oldCategoryId != command.CategoryId.Value)
            changes.Add($"CategoryId: {oldCategoryId} → {command.CategoryId.Value}");

        if (changes.Any())
        {
            _logger.LogWarning(
                //"Product updated: ProductId {ProductId}, Name {Name}, Changes: {Changes}, AdminId {AdminId}, Time {Time}",
                "Product updated: ProductId {ProductId}, Name {Name}, Changes: {Changes}, Time {Time}",
                product.Id,
                product.Name,
                string.Join("; ", changes),
                //GetCurrentAdminId(),
                DateTime.UtcNow
            );
        }
        else
        {
            _logger.LogInformation(
                "Product update completed with no changes: ProductId {ProductId}, Name {Name}",
                product.Id,
                product.Name
            );
        }

        // ✅ Очищаем кэш
        await _cacheService.RemoveAsync($"product:{command.ProductId}");
        await _cacheService.RemoveByPrefix("products:");

    }
}
