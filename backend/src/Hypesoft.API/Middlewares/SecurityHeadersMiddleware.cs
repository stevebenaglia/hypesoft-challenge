namespace Hypesoft.API.Middlewares;

public sealed class SecurityHeadersMiddleware
{
    // CSP for a JSON API that also serves Swagger UI.
    // • script-src / style-src 'unsafe-inline' – required by Swagger UI's bundled JS/CSS.
    // • img-src data: – Swagger logo is embedded as a data URI.
    // • connect-src 'self' – lets Swagger make API calls from the same origin.
    // • frame-ancestors 'none' – stricter than X-Frame-Options; kept for legacy compat.
    private const string Csp =
        "default-src 'none'; " +
        "script-src 'self' 'unsafe-inline'; " +
        "style-src 'self' 'unsafe-inline'; " +
        "img-src 'self' data:; " +
        "connect-src 'self'; " +
        "font-src 'self'; " +
        "frame-ancestors 'none'";

    private readonly RequestDelegate _next;

    public SecurityHeadersMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var headers = context.Response.Headers;

        headers.Remove("Server");
        headers["X-Content-Type-Options"] = "nosniff";
        headers["X-Frame-Options"] = "DENY";
        headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
        headers["Content-Security-Policy"] = Csp;
        headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";
        headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains";

        await _next(context);
    }
}
