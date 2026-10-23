using Application.Commands.Order;
using FluentValidation;

namespace Application.Validators.Order;

public class ReceiveOrderCommandValidator : AbstractValidator<ReceiveOrderCommand>
{
    public ReceiveOrderCommandValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0).WithMessage("User ID is required");

        RuleFor(x => x.OrderId)
            .GreaterThan(0).WithMessage("Order ID is required");

        RuleFor(x => x.BaseUrl)
            .NotEmpty().WithMessage("Base URL is required");
    }
}
