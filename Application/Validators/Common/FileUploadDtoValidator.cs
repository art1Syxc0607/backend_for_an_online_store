using Application.DTOs.File;
using FluentValidation;

namespace Application.Validators.Common;

public class FileUploadDtoValidator : AbstractValidator<FileUploadDto>
{
    public FileUploadDtoValidator()
    {
        RuleFor(x => x.Stream)
            .NotNull().WithMessage("File stream is required");

        RuleFor(x => x.FileName)
            .NotEmpty().WithMessage("File name is required")
            .MaximumLength(255).WithMessage("File name cannot exceed 255 characters");

        RuleFor(x => x.ContentType)
            .NotEmpty().WithMessage("File content type is required")
            .Must(AllowedFileTypes.IsAllowed).WithMessage("Unsupported file type");

        RuleFor(x => x.Length)
            .GreaterThan(0).WithMessage("File is empty")
            .LessThanOrEqualTo(AllowedFileTypes.MaxFileSizeBytes)
                .WithMessage($"File exceeds {AllowedFileTypes.MaxFileSizeMb}MB");
    }
}
