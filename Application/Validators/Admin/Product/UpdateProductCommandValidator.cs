using Application.Commands.Admin.Product;
using FluentValidation;

namespace Application.Validators.Admin.Product;

/// <summary>
/// Валидация команды обновления товара. Все поля опциональны:
/// проверяем только то, что реально пришло в запросе.
/// </summary>
public class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    private const int MaxNameLength = 70;
    private const int MaxDescriptionLength = 500;
    private const int MaxSkuLength = 100;

    public UpdateProductCommandValidator()
    {
        RuleFor(x => x.ProductId)
            .GreaterThan(0).WithMessage("Product ID is required");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Product name cannot be empty")
            .MaximumLength(MaxNameLength)
                .WithMessage($"Product name cannot exceed {MaxNameLength} characters")
            .When(x => x.Name != null);

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Product description cannot be empty")
            .MaximumLength(MaxDescriptionLength)
                .WithMessage($"Product description cannot exceed {MaxDescriptionLength} characters")
            .When(x => x.Description != null);

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Price must be greater than zero")
            .When(x => x.Price.HasValue);

        RuleFor(x => x.PurchasePrice)
            .GreaterThanOrEqualTo(0).WithMessage("Purchase price cannot be negative")
            .When(x => x.PurchasePrice.HasValue);

        RuleFor(x => x.StockQuantity)
            .GreaterThanOrEqualTo(0).WithMessage("Stock quantity cannot be negative")
            .When(x => x.StockQuantity.HasValue);

        RuleFor(x => x.ReservedQuantity)
            .GreaterThanOrEqualTo(0).WithMessage("Reserved quantity cannot be negative")
            .When(x => x.ReservedQuantity.HasValue);

        RuleFor(x => x.Sku)
            .MaximumLength(MaxSkuLength)
                .WithMessage($"SKU cannot exceed {MaxSkuLength} characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Sku));

        RuleFor(x => x.CategoryId)
            .GreaterThan(0).WithMessage("Category ID must be positive")
            .When(x => x.CategoryId.HasValue);
    }
}
