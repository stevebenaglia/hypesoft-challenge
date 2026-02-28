namespace Hypesoft.API.Extensions;

public static class CorsExtensions
{
    public const string FrontendPolicy = "AllowFrontend";

    public static IServiceCollection AddFrontendCors(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy(FrontendPolicy, policy =>
                policy
                    .WithOrigins("http://localhost", "http://localhost:80", "http://localhost:3000")
                    .AllowAnyMethod()
                    .AllowAnyHeader());
        });

        return services;
    }
}
