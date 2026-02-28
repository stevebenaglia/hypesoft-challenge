using System.Diagnostics;
using System.Text.Json;
using FluentValidation;
using Hypesoft.Domain.Exceptions;

namespace Hypesoft.API.Middlewares;

public sealed class ExceptionHandlingMiddleware
{
    private static readonly JsonSerializerOptions JsonOptions =
        new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

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
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Request cancelled: {Method} {Path}", context.Request.Method, context.Request.Path);
            // client disconnected — do not write a response
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning("Validation failed for request {Path}", context.Request.Path);
            await WriteErrorResponse(context, StatusCodes.Status422UnprocessableEntity,
                "Validation Error", BuildValidationErrors(ex));
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning("Resource not found: {Message}", ex.Message);
            await WriteErrorResponse(context, StatusCodes.Status404NotFound, ex.Message);
        }
        catch (DomainException ex)
        {
            _logger.LogWarning("Domain error: {Message}", ex.Message);
            await WriteErrorResponse(context, StatusCodes.Status400BadRequest, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception for request {Path}", context.Request.Path);
            await WriteErrorResponse(context, StatusCodes.Status500InternalServerError,
                "An unexpected error occurred.");
        }
    }

    private static async Task WriteErrorResponse(
        HttpContext context,
        int statusCode,
        string title,
        Dictionary<string, string[]>? errors = null)
    {
        if (context.Response.HasStarted)
            return;

        var traceId = Activity.Current?.Id ?? context.TraceIdentifier;

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        var response = new { title, status = statusCode, traceId, errors };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, JsonOptions));
    }

    private static Dictionary<string, string[]> BuildValidationErrors(ValidationException ex)
        => ex.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.ErrorMessage).ToArray());
}
