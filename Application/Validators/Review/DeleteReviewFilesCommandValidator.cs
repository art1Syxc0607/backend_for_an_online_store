using Application.Commands.Review;
using Application.Validators.Common;
using FluentValidation;

namespace Application.Validators.Review;

public class DeleteReviewFilesCommandValidator : AbstractValidator<DeleteReviewFilesCommand>
{
    public DeleteReviewFilesCommandValidator()
    {
        RuleFor(x => x.ReviewId)
            .GreaterThan(0).WithMessage("Review ID is required");

        RuleFor(x => x.UserId)
            .GreaterThan(0).WithMessage("User ID is required");

        RuleFor(x => x.FileUrls)
            .NotNull().WithMessage("File URLs are required")
            .Must(urls => urls.Count > 0).WithMessage("No file URLs provided")
            .Must(urls => urls.Count <= AllowedFileTypes.MaxFiles)
            .WithMessage($"Maximum {AllowedFileTypes.MaxFiles} files allowed");

        RuleForEach(x => x.FileUrls)
            .NotEmpty().WithMessage("File URL cannot be empty")
            .MaximumLength(500).WithMessage("File URL cannot exceed 500 characters");
    }
}
