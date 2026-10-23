using Application.Commands.Order;
using FluentValidation;

namespace Application.Validators.Order;

public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    private const int MaxItems = 100;
    private const int MaxQuantity = 10_000;

    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0).WithMessage("User ID is required");

        RuleFor(x => x.ShippingAddress)
            .NotEmpty().WithMessage("Shipping address is required")
            .MinimumLength(5).WithMessage("Shipping address must be at least 5 characters")
            .MaximumLength(500).WithMessage("Shipping address cannot exceed 500 characters");

        RuleFor(x => x.BaseUrl)
            .NotEmpty().WithMessage("Base URL is required");

        RuleFor(x => x.Items)
            .NotNull().WithMessage("Items are required")
            .Must(items => items.Count > 0).WithMessage("At least one item is required")
            .Must(items => items.Count <= MaxItems).WithMessage($"Maximum {MaxItems} items per order");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ProductId)
                .GreaterThan(0).WithMessage("Product ID is required");

            item.RuleFor(i => i.Quantity)
                .InclusiveBetween(1, MaxQuantity)
                .WithMessage($"Quantity must be between 1 and {MaxQuantity}");
        });
    }
}
