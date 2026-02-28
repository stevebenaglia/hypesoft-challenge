using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace Hypesoft.API.Extensions;

public static class HealthCheckExtensions
{
    public static IServiceCollection AddAppHealthChecks(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration["MongoDB:ConnectionString"]
            ?? throw new InvalidOperationException("MongoDB:ConnectionString is not configured.");

        services.AddHealthChecks()
            .AddMongoDb(
                connectionString,
                name: "mongodb",
                timeout: TimeSpan.FromSeconds(3),
                tags: ["ready"]);

        return services;
    }

    public static IEndpointRouteBuilder MapAppHealthChecks(this IEndpointRouteBuilder app)
    {
        app.MapHealthChecks("/health/live");

        app.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("ready")
        });

        return app;
    }
}
