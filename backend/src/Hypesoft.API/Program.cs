using Hypesoft.Application.Extensions;
using Hypesoft.API.Extensions;
using Hypesoft.API.Middlewares;
using Hypesoft.Infrastructure.Configurations;
using Hypesoft.Infrastructure.Data;
using Microsoft.AspNetCore.ResponseCompression;
using Serilog;
using Serilog.Formatting.Compact;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
    options.Limits.MaxRequestBodySize = 1 * 1024 * 1024); // 1 MB

// Logging
builder.Host.UseSerilog((ctx, lc) => lc
    .ReadFrom.Configuration(ctx.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "Hypesoft.API")
    .WriteTo.Console(new CompactJsonFormatter()));

// Application layers
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

// Authentication + Authorization
builder.Services.AddKeycloakAuthentication(builder.Configuration);

// API
builder.Services.AddControllers();

// Swagger
builder.Services.AddSwaggerWithJwt();

// CORS
builder.Services.AddFrontendCors();

// Health Checks
builder.Services.AddAppHealthChecks(builder.Configuration);

// Rate Limiting
builder.Services.AddAppRateLimiting();

// Response Compression
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await db.Database.EnsureCreatedAsync();
}

app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseSerilogRequestLogging(options =>
{
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("User", httpContext.User.Identity?.Name ?? "anonymous");
        diagnosticContext.Set("RemoteIp", httpContext.Connection.RemoteIpAddress?.ToString());
    };
});

app.UseResponseCompression();

app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Hypesoft API v1"));

app.UseCors(CorsExtensions.FrontendPolicy);
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapAppHealthChecks();

app.Run();
