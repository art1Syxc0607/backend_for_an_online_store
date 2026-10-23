using FluentValidation;

namespace Application.Validators.Common;

/// <summary>
/// Единые правила пароля, чтобы Register и ResetPassword не расходились.
/// </summary>
internal static class PasswordRules
{
    public const int MinLength = 6;
    public const int MaxLength = 100;

    private const string ComplexityPattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{6,}$";

    public static IRuleBuilderOptions<T, string> ValidPassword<T>(
        this IRuleBuilder<T, string> rule)
        => rule
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(MinLength)
                .WithMessage($"Password must be at least {MinLength} characters")
            .MaximumLength(MaxLength)
                .WithMessage($"Password cannot exceed {MaxLength} characters")
            .Matches(ComplexityPattern)
                .WithMessage("Password must contain at least one uppercase, one lowercase, and one number");
}
