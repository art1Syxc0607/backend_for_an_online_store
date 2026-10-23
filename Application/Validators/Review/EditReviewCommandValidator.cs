using Application.Commands.Review;
using FluentValidation;

namespace Application.Validators.Review;

public class EditReviewCommandValidator : AbstractValidator<EditReviewCommamd>
{
    public EditReviewCommandValidator()
    {
        RuleFor(x => x.ReviewId)
            .GreaterThan(0).WithMessage("Review ID is required");

        RuleFor(x => x.UserId)
            .GreaterThan(0).WithMessage("User ID is required");

        RuleFor(x => x.NewText)
            .NotEmpty().WithMessage("Review text cannot be empty")
            .MinimumLength(3).WithMessage("Review must be at least 3 characters")
            .MaximumLength(2000).WithMessage("Review cannot exceed 2000 characters")
            .When(x => x.NewText is not null);

        RuleFor(x => x.NewRating)
            .InclusiveBetween(1, 5).WithMessage("Rating must be between 1 and 5")
            .When(x => x.NewRating.HasValue);

        RuleFor(x => x)
            .Must(x => x.NewText is not null || x.NewRating.HasValue)
            .WithName("request")
            .WithMessage("Nothing to update: provide new text or rating");
    }
}
