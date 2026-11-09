// WebAPI/Middleware/ExceptionHandlingMiddleware.cs
using Application.DTOs.Error;
using Domain.Exceptions;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using SendGrid.Helpers.Errors.Model;
using Serilog;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;

namespace WebAPI.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        // 1. Логируем исключение.
        // Ожидаемые клиентские ошибки (4xx) — Warning, без засорения error-логов.
        if (exception is DomainException domainException && domainException.StatusCode < 500)
        {
            _logger.LogWarning(
                exception,
                "Request rejected: {Message}, Path: {Path}",
                exception.Message,
                context.Request.Path
            );
        }
        else
        {
            _logger.LogError(
                exception,
                "Unhandled exception occurred: {Message}, Path: {Path}",
                exception.Message,
                context.Request.Path
            );
        }

        // 2. Преобразуем в понятный ответ
        var response = CreateErrorResponse(exception, context);

        // 3. Отправляем клиенту
        context.Response.StatusCode = response.StatusCode;
        context.Response.ContentType = "application/json";

        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }

    private ErrorResponseDto CreateErrorResponse(Exception exception, HttpContext context)
    {
        var traceId = context.TraceIdentifier;

        // 4. Обрабатываем известные типы исключений
        return exception switch
        {
            // Ошибка валидации (FluentValidation)
            Domain.Exceptions.ValidationException validationEx => new ErrorResponseDto
            {
                Error = validationEx.Message,
                ErrorCode = validationEx.ErrorCode ?? "VALIDATION_ERROR",
                StatusCode = 400,
                Timestamp = DateTime.UtcNow,
                Path = context.Request.Path,
                TraceId = traceId,
                ValidationErrors = validationEx.Errors.Select(e => new ValidationError
                {
                    Field = e.Field,
                    Message = e.Message
                }).ToList()
            },

            // Не найдено
            Domain.Exceptions.NotFoundException notFoundEx => new ErrorResponseDto
            {
                Error = notFoundEx.Message,
                ErrorCode = "NOT_FOUND",
                StatusCode = 404,
                Timestamp = DateTime.UtcNow,
                Path = context.Request.Path,
                TraceId = traceId
            },

            // Неавторизован
            Domain.Exceptions.UnauthorizedException unauthEx => new ErrorResponseDto
            {
                Error = unauthEx.Message,
                ErrorCode = "UNAUTHORIZED",
                StatusCode = 401,
                Timestamp = DateTime.UtcNow,
                Path = context.Request.Path,
                TraceId = traceId
            },

            // Недостаточно прав
            Domain.Exceptions.ForbiddenException forbiddenEx => new ErrorResponseDto
            {
                Error = forbiddenEx.Message,
                ErrorCode = "FORBIDDEN",
                StatusCode = 403,
                Timestamp = DateTime.UtcNow,
                Path = context.Request.Path,
                TraceId = traceId
            },

            // Бизнес-ошибки
            DomainException domainEx => new ErrorResponseDto
            {
                Error = domainEx.Message,
                ErrorCode = domainEx.ErrorCode,
                StatusCode = domainEx.StatusCode,
                Timestamp = DateTime.UtcNow,
                Path = context.Request.Path,
                TraceId = traceId
            },

            // Конфликт конкурентного обновления (xmin / row version mismatch)
            Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException concurrencyEx => new ErrorResponseDto
            {
                Error = "The record was modified by another request. Please reload and try again.",
                ErrorCode = "CONCURRENCY_ERROR",
                StatusCode = 409,
                Timestamp = DateTime.UtcNow,
                Path = context.Request.Path,
                TraceId = traceId
            },

            // Нарушение уникального ограничения (23505) — дубль в избранном, отзыве и т.д.
            Npgsql.PostgresException pgEx when pgEx.SqlState == "23505" => new ErrorResponseDto
            {
                Error = "Запись уже существует.",
                ErrorCode = "DUPLICATE_ENTRY",
                StatusCode = 409,
                Timestamp = DateTime.UtcNow,
                Path = context.Request.Path,
                TraceId = traceId
            },

            // Нарушение уникального ограничения, обёрнутое EF Core в DbUpdateException.
            // SaveChanges() почти всегда приходит именно так: PostgresException лежит во InnerException.
            DbUpdateException dupEx when IsUniqueViolation(dupEx) => new ErrorResponseDto
            {
                Error = "Запись уже существует.",
                ErrorCode = "DUPLICATE_ENTRY",
                StatusCode = 409,
                Timestamp = DateTime.UtcNow,
                Path = context.Request.Path,
                TraceId = traceId
            },

            // Ошибка базы данных
            DbUpdateException dbEx => new ErrorResponseDto
            {
                Error = "A database error occurred. Please try again.",
                ErrorCode = "DATABASE_ERROR",
                StatusCode = 500,
                Timestamp = DateTime.UtcNow,
                Path = context.Request.Path,
                TraceId = traceId
            },

            // Всё остальное (неизвестные ошибки)
            _ => new ErrorResponseDto
            {
                Error = "An unexpected error occurred. Please try again later.",
                ErrorCode = "INTERNAL_SERVER_ERROR",
                StatusCode = 500,
                Timestamp = DateTime.UtcNow,
                Path = context.Request.Path,
                TraceId = traceId
            }
        };
    }

    /// <summary>
    /// Проверяет цепочку InnerException на нарушение уникальности PostgreSQL (23505).
    /// EF Core оборачивает PostgresException в DbUpdateException при SaveChanges.
    /// </summary>
    private static bool IsUniqueViolation(Exception exception)
    {
        for (var current = exception; current != null; current = current.InnerException)
        {
            if (current is PostgresException pg && pg.SqlState == "23505")
                return true;
        }

        return false;
    }
}