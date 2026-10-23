using Application.Common;
using Application.Queries.Admin.Dashboard;
using FluentValidation;

namespace Application.Validators.Admin.Dashboard;

public class GetRevenueForThePeriodCommandValidator
    : AbstractValidator<GetRevenueForThePeriodCommand>
{
    public GetRevenueForThePeriodCommandValidator()
    {
        // В команде исторически опечатка в именах свойств: Priod вместо Period.
        RuleFor(x => x.LastDayOfThePriod)
            .Must((command, lastDay) => DateRange.IsValid(command.FirstDayOfThePriod, lastDay))
            .WithMessage("First day of the period cannot be later than last day");
    }
}
