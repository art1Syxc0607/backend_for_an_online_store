namespace Domain.Exceptions;

/// <summary>
/// Одна ошибка валидации: имя поля и сообщение.
/// </summary>
public sealed record ValidationErrorDetail(string Field, string Message);

public class ValidationException : DomainException
{
    public IReadOnlyList<ValidationErrorDetail> Errors { get; }

    public ValidationException(IEnumerable<ValidationErrorDetail> errors)
        : base("Validation failed", "VALIDATION_ERROR", 400)
    {
        Errors = errors.ToList();
    }
}
