using Application.Commands.Cart;
using FluentValidation;

namespace Application.Validators.Cart;

public class AddToCartCommandValidator : AbstractValidator<AddToCartCommand>
{
    private const int MaxQuantity = 10_000;

    public AddToCartCommandValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0).WithMessage("User ID is required");

        RuleFor(x => x.ProductId)
            .GreaterThan(0).WithMessage("Product ID is required");

        RuleFor(x => x.Quantity)
            .InclusiveBetween(1, MaxQuantity)
            .WithMessage($"Quantity must be between 1 and {MaxQuantity}");
    }
}
