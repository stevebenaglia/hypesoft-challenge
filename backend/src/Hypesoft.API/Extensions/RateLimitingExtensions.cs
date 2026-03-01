using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

namespace Hypesoft.API.Extensions;

public static class RateLimitingExtensions
{
    public const string DefaultPolicy = "default";
    public const string WritesPolicy = "writes";

    public static IServiceCollection AddAppRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.AddFixedWindowLimiter(DefaultPolicy, cfg =>
            {
                cfg.Window = TimeSpan.FromSeconds(60);
                cfg.PermitLimit = 100;
                cfg.QueueLimit = 0;
                cfg.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            });

            options.AddFixedWindowLimiter(WritesPolicy, cfg =>
            {
                cfg.Window = TimeSpan.FromSeconds(60);
                cfg.PermitLimit = 20;
                cfg.QueueLimit = 0;
                cfg.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            });

            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        });

        return services;
    }
}
