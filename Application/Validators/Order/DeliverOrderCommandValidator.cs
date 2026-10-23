using Application.Commands.Order;
using FluentValidation;

namespace Application.Validators.Order;

public class DeliverOrderCommandValidator : AbstractValidator<DeliverOrderCommand>
{
    public DeliverOrderCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .GreaterThan(0).WithMessage("Order ID is required");

        RuleFor(x => x.BaseUrl)
            .NotEmpty().WithMessage("Base URL is required");
    }
}
