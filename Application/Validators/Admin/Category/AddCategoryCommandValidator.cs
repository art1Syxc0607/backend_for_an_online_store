using Application.Commands.Admin.Category;
using FluentValidation;

namespace Application.Validators.Admin.Category;

/// <summary>
/// Валидация команды создания категории. Лимит длины имени совпадает
/// с доменным правилом Category.SetName.
/// </summary>
public class AddCategoryCommandValidator : AbstractValidator<AddCategoryCommand>
{
    private const int MaxNameLength = 100;
    private const int MaxDescriptionLength = 500;

    public AddCategoryCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Category name is required")
            .MaximumLength(MaxNameLength)
                .WithMessage($"Category name cannot exceed {MaxNameLength} characters");

        RuleFor(x => x.Description)
            .MaximumLength(MaxDescriptionLength)
                .WithMessage($"Category description cannot exceed {MaxDescriptionLength} characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Description));
    }
}
