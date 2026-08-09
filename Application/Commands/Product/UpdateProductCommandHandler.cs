using Application.Interfaces;
using MediatR;
using Domain.Exceptions;

namespace Application.Commands.Product;

public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand>
{
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICategoryRepository _categoryRepository;

    public UpdateProductCommandHandler(IProductRepository productRepository, IUnitOfWork unitOfWork,
        ICategoryRepository cartRepository)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
        _categoryRepository = cartRepository;
    }


    public async Task Handle(UpdateProductCommand command, CancellationToken ct)
    {
        var product = await _productRepository.GetByIdAsync(command.ProductId, ct);
        if (product == null)
            throw new DomainException("Product not found");

        // Проверяем категорию
        if (command.CategoryId.HasValue)
        {
            var category = await _categoryRepository.GetByIdAsync(command.CategoryId.Value, ct);
            if (category == null)
                throw new DomainException($"Category {command.CategoryId} not found");
            product.AssignCategory(category);
        }


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
    }
}
