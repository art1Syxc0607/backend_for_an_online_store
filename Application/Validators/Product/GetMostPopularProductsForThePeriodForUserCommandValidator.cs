using Application.Commands.Product;
using Application.Common;
using FluentValidation;

namespace Application.Validators.Product;

public class GetMostPopularProductsForThePeriodForUserCommandValidator
    : AbstractValidator<GetMostPopularProductsForThePeriodForUserCommand>
{
    public GetMostPopularProductsForThePeriodForUserCommandValidator()
    {
        RuleFor(x => x.LastDayOfThePeriod)
            .Must((command, lastDay) => DateRange.IsValid(command.FirstDayOfThePeriod, lastDay))
            .WithMessage("First day of the period cannot be later than last day");

        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1).WithMessage("Page number must be at least 1")
            .When(x => x.PageNumber.HasValue);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 50).WithMessage("Page size must be between 1 and 50")
            .When(x => x.PageSize.HasValue);
    }
}
