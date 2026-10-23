using Application.Commands.Cart;
using FluentValidation;

namespace Application.Validators.Cart;

public class UpdateCartItemQuantityCommandValidator : AbstractValidator<UpdateCartItemQuantityCommand>
{
    private const int MaxQuantity = 10_000;

    public UpdateCartItemQuantityCommandValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0).WithMessage("User ID is required");

        RuleFor(x => x.ProductId)
            .GreaterThan(0).WithMessage("Product ID is required");

        // 0 означает удаление позиции из корзины
        RuleFor(x => x.NewQuantity)
            .InclusiveBetween(0, MaxQuantity)
            .WithMessage($"Quantity must be between 0 and {MaxQuantity}");
    }
}
