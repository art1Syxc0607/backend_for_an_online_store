using Application.Commands.Review;
using Application.Validators.Common;
using FluentValidation;

namespace Application.Validators.Review;

public class AddReviewCommandValidator : AbstractValidator<AddReviewCommand>
{
    public AddReviewCommandValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0).WithMessage("User ID is required");

        RuleFor(x => x.ProductId)
            .GreaterThan(0).WithMessage("Product ID is required");

        RuleFor(x => x.Text)
            .NotEmpty().WithMessage("Review text is required")
            .MinimumLength(3).WithMessage("Review must be at least 3 characters")
            .MaximumLength(2000).WithMessage("Review cannot exceed 2000 characters");

        RuleFor(x => x.Rating)
            .InclusiveBetween(1, 5).WithMessage("Rating must be between 1 and 5");

        RuleFor(x => x.Files)
            .Must(files => files is null || files.Count <= AllowedFileTypes.MaxFiles)
            .WithMessage($"Maximum {AllowedFileTypes.MaxFiles} files allowed");

        RuleForEach(x => x.Files).SetValidator(new FileUploadDtoValidator());
    }
}
