using Application.Commands.Admin.Category;
using FluentValidation;

namespace Application.Validators.Admin.Category;

/// <summary>
/// Валидация команды обновления категории. Имя и описание опциональны:
/// если поле не пришло, домен оставляет текущее значение.
/// </summary>
public class UpdateCategoryCommandValidator : AbstractValidator<UpdateCategoryCommand>
{
    private const int MaxNameLength = 100;
    private const int MaxDescriptionLength = 500;

    public UpdateCategoryCommandValidator()
    {
        RuleFor(x => x.CategoryId)
            .GreaterThan(0).WithMessage("Category ID is required");

        RuleFor(x => x.CategoryName)
            .NotEmpty().WithMessage("Category name cannot be empty")
            .MaximumLength(MaxNameLength)
                .WithMessage($"Category name cannot exceed {MaxNameLength} characters")
            .When(x => x.CategoryName != null);

        RuleFor(x => x.CategoryDescription)
            .MaximumLength(MaxDescriptionLength)
                .WithMessage($"Category description cannot exceed {MaxDescriptionLength} characters")
            .When(x => !string.IsNullOrWhiteSpace(x.CategoryDescription));
    }
}
