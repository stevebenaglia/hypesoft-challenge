using Microsoft.AspNetCore.Mvc.Filters;

namespace Hypesoft.API.Filters;

/// <summary>
/// Action filter that logs incoming HTTP requests and their responses.
/// </summary>
public sealed class RequestLoggingFilter : IActionFilter
{
    private readonly ILogger<RequestLoggingFilter> _logger;

    public RequestLoggingFilter(ILogger<RequestLoggingFilter> logger)
    {
        _logger = logger;
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        var request = context.HttpContext.Request;
        var user = context.HttpContext.User.Identity?.Name ?? "anonymous";

        _logger.LogInformation(
            "Incoming request: {Method} {Path} | User: {User}",
            request.Method,
            request.Path,
            user);
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        var request = context.HttpContext.Request;
        var statusCode = context.HttpContext.Response.StatusCode;

        _logger.LogInformation(
            "Completed request: {Method} {Path} | Status: {StatusCode}",
            request.Method,
            request.Path,
            statusCode);
    }
}
