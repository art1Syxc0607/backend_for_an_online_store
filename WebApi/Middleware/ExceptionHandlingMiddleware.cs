// WebAPI/Middleware/ExceptionHandlingMiddleware.cs
using Application.DTOs.Error;
using Domain.Exceptions;
using Microsoft.EntityFrameworkCore;
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
        // 1. Логируем исключение (со всей информацией для разработчика)
        _logger.LogError(
            exception,
            "Unhandled exception occurred: {Message}, Path: {Path}",
            exception.Message,
            context.Request.Path
        );

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
                ValidationErrors = validationEx.Errors?.Select(e => new ValidationError
                {
                    Field = e.Split(':')[0]?.Trim() ?? "",
                    Message = e
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
}