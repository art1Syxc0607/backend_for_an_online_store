using Application.Commands.Order;
using FluentValidation;

namespace Application.Validators.Order;

public class ConfirmPaymentCommandValidator : AbstractValidator<ConfirmPaymentCommand>
{
    public ConfirmPaymentCommandValidator()
    {
        RuleFor(x => x.PaymentIntentId)
            .NotEmpty().WithMessage("Payment intent ID is required")
            .MaximumLength(200).WithMessage("Payment intent ID cannot exceed 200 characters");

        RuleFor(x => x.UserId)
            .GreaterThan(0).WithMessage("User ID is required");

        RuleFor(x => x.BaseUrl)
            .NotEmpty().WithMessage("Base URL is required");
    }
}
