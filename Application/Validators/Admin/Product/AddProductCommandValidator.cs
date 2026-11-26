using Application.Commands.Admin.Product;
using FluentValidation;

namespace Application.Validators.Admin.Product;

/// <summary>
/// Валидация команды создания товара. Лимиты длины совпадают с
/// ProductConfiguration, чтобы не доводить дело до ошибки PostgreSQL.
/// </summary>
public class AddProductCommandValidator : AbstractValidator<AddProductCommand>
{
    private const int MaxNameLength = 70;
    private const int MaxDescriptionLength = 500;
    private const int MaxSkuLength = 100;

    public AddProductCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Product name is required")
            .MaximumLength(MaxNameLength)
                .WithMessage($"Product name cannot exceed {MaxNameLength} characters");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Product description is required")
            .MaximumLength(MaxDescriptionLength)
                .WithMessage($"Product description cannot exceed {MaxDescriptionLength} characters");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Price must be greater than zero");

        RuleFor(x => x.PurchasePrice)
            .GreaterThanOrEqualTo(0).WithMessage("Purchase price cannot be negative")
            .LessThanOrEqualTo(x => x.Price)
                .WithMessage("Purchase price cannot be higher than selling price");

        RuleFor(x => x.StockQuantity)
            .GreaterThanOrEqualTo(0).WithMessage("Stock quantity cannot be negative");

        RuleFor(x => x.Sku)
            .MaximumLength(MaxSkuLength)
                .WithMessage($"SKU cannot exceed {MaxSkuLength} characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Sku));

        RuleFor(x => x.CategoryId)
            .GreaterThan(0).WithMessage("Category ID must be positive")
            .When(x => x.CategoryId.HasValue);
    }
}
