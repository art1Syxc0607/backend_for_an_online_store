using Application.Commands.User;
using Application.Validators.Common;
using FluentValidation;

namespace Application.Validators.User;

public class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(100).WithMessage("Email cannot exceed 100 characters");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Reset code is required")
            .Matches(@"^\d{6}$").WithMessage("Reset code must contain exactly 6 digits");

        RuleFor(x => x.NewPassword).ValidPassword();

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty().WithMessage("Password confirmation is required")
            .Equal(x => x.NewPassword).WithMessage("Passwords do not match");
    }
}
