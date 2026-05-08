using System.Net;
using System.Text.Json;
using FluentValidation;
using TaskManager.Domain.Exceptions;

namespace TaskManager.Api.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
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
        var (statusCode, message, errors) = exception switch
        {
            ValidationException ve => (
                HttpStatusCode.BadRequest,
                "Validation failed.",
                (object)ve.Errors.Select(e => new { field = e.PropertyName, message = e.ErrorMessage })
            ),
            NotFoundException => (HttpStatusCode.NotFound, exception.Message, (object?)null),
            UnauthorizedException => (HttpStatusCode.Unauthorized, exception.Message, (object?)null),
            DomainException => (HttpStatusCode.UnprocessableEntity, exception.Message, (object?)null),
            _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred.", (object?)null)
        };

        if (statusCode == HttpStatusCode.InternalServerError)
            _logger.LogError(exception, "Unhandled exception");

        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";

        var response = errors is not null
            ? new { message, errors }
            : (object)new { message };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
    }
}
