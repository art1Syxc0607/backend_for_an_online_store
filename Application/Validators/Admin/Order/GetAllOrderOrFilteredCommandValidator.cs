using Application.Common;
using Application.Queries.Admin.Order;
using FluentValidation;

namespace Application.Validators.Admin.Order;

public class GetAllOrderOrFilteredCommandValidator
    : AbstractValidator<GetAllOrderOrFilteredCommand>
{
    public GetAllOrderOrFilteredCommandValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0).WithMessage("User ID must be greater than 0")
            .When(x => x.UserId.HasValue);

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
