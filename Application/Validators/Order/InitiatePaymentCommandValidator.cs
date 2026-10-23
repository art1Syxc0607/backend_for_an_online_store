using Application.Commands.Order;
using FluentValidation;

namespace Application.Validators.Order;

public class InitiatePaymentCommandValidator : AbstractValidator<InitiatePaymentCommand>
{
    public InitiatePaymentCommandValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0).WithMessage("User ID is required");

        RuleFor(x => x.OrderId)
            .GreaterThan(0).WithMessage("Order ID is required");

        RuleFor(x => x.Method)
            .IsInEnum().WithMessage("Unknown payment method");
    }
}
