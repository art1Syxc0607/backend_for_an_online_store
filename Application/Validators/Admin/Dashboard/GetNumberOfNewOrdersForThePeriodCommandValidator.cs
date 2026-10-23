using Application.Common;
using Application.Queries.Admin.Dashboard;
using FluentValidation;

namespace Application.Validators.Admin.Dashboard;

public class GetNumberOfNewOrdersForThePeriodCommandValidator
    : AbstractValidator<GetNumberOfNewOrdersForThePeriodCommand>
{
    public GetNumberOfNewOrdersForThePeriodCommandValidator()
    {
        // В команде исторически опечатка в именах свойств: Priod вместо Period.
        RuleFor(x => x.LastDayOfThePriod)
            .Must((command, lastDay) => DateRange.IsValid(command.FirstDayOfThePriod, lastDay))
            .WithMessage("First day of the period cannot be later than last day");
    }
}
