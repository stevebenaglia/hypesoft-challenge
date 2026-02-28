using Hypesoft.Application.Extensions;
using Hypesoft.API.Extensions;
using Hypesoft.API.Filters;
using Hypesoft.API.Middlewares;
using Hypesoft.Infrastructure.Configurations;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Logging
builder.Host.UseSerilog((ctx, lc) => lc
    .ReadFrom.Configuration(ctx.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console());

// Application layers
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

// Authentication + Authorization
builder.Services.AddKeycloakAuthentication(builder.Configuration);

// API
builder.Services.AddControllers(options =>
{
    options.Filters.Add<RequestLoggingFilter>();
});

// Swagger
builder.Services.AddSwaggerWithJwt();

// CORS
builder.Services.AddFrontendCors();

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Hypesoft API v1"));

app.UseCors(CorsExtensions.FrontendPolicy);
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
