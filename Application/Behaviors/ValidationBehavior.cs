using Domain.Exceptions;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using DomainValidationException = Domain.Exceptions.ValidationException;

namespace Application.Behaviors;

/// <summary>
/// Pipeline behavior: валидирует команды и запросы через FluentValidation
/// ДО вызова handler'а (и до кэша, т.к. регистрируется самым внешним).
/// </summary>
public class ValidationBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;
    private readonly ILogger<ValidationBehavior<TRequest, TResponse>> _logger;

    public ValidationBehavior(
        IEnumerable<IValidator<TRequest>> validators,
        ILogger<ValidationBehavior<TRequest, TResponse>> logger)
    {
        _validators = validators;
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        // Быстрый путь: для запроса нет валидаторов
        if (!_validators.Any())
        {
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);

        var results = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, ct)));

        var failures = results
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToList();

        if (failures.Count == 0)
        {
            return await next();
        }

        var errors = failures
            .Select(f => new ValidationErrorDetail(
                string.IsNullOrWhiteSpace(f.PropertyName) ? "request" : f.PropertyName,
                f.ErrorMessage))
            .ToList();

        _logger.LogDebug(
            "Validation failed for {RequestName}: {ErrorCount} error(s)",
            typeof(TRequest).Name,
            errors.Count);

        throw new DomainValidationException(errors);
    }
}
