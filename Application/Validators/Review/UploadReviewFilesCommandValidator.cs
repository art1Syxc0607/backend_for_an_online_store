using Application.Commands.Review;
using Application.Validators.Common;
using FluentValidation;

namespace Application.Validators.Review;

public class UploadReviewFilesCommandValidator : AbstractValidator<UploadReviewFilesCommand>
{
    public UploadReviewFilesCommandValidator()
    {
        RuleFor(x => x.ReviewId)
            .GreaterThan(0).WithMessage("Review ID is required");

        RuleFor(x => x.UserId)
            .GreaterThan(0).WithMessage("User ID is required");

        RuleFor(x => x.Files)
            .NotNull().WithMessage("Files are required")
            .Must(files => files.Count > 0).WithMessage("At least one file is required")
            .Must(files => files.Count <= AllowedFileTypes.MaxFiles)
            .WithMessage($"Maximum {AllowedFileTypes.MaxFiles} files allowed");

        RuleForEach(x => x.Files).SetValidator(new FileUploadDtoValidator());
    }
}
