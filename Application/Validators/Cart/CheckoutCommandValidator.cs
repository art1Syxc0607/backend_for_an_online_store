using Application.Commands.Cart;
using FluentValidation;

namespace Application.Validators.Cart;

public class CheckoutCommandValidator : AbstractValidator<CheckoutCommand>
{
    public CheckoutCommandValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0).WithMessage("User ID is required");

        RuleFor(x => x.ShippingAddress)
            .NotEmpty().WithMessage("Shipping address is required")
            .MinimumLength(5).WithMessage("Shipping address must be at least 5 characters")
            .MaximumLength(500).WithMessage("Shipping address cannot exceed 500 characters");

        RuleFor(x => x.BaseUrl)
            .NotEmpty().WithMessage("Base URL is required");
    }
}
