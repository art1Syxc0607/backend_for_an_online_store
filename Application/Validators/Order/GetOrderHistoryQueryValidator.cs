using Application.Common;
using Application.Queries.Order;
using FluentValidation;

namespace Application.Validators.Order;

public class GetOrderHistoryQueryValidator : AbstractValidator<GetOrderHistoryQuery>
{
    public GetOrderHistoryQueryValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0).WithMessage("User ID is required");

        RuleFor(x => x.LastDayOfThePeriod)
            .Must((query, lastDay) => DateRange.IsValid(query.FirstDayOfThePeriod, lastDay))
            .WithMessage("First day of the period cannot be later than last day");

        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1).WithMessage("Page number must be at least 1")
            .When(x => x.PageNumber.HasValue);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 50).WithMessage("Page size must be between 1 and 50")
            .When(x => x.PageSize.HasValue);
    }
}
