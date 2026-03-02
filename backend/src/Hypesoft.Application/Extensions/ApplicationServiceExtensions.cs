using FluentValidation;
using Hypesoft.Application.Behaviors;
using Hypesoft.Application.Interfaces;
using Hypesoft.Application.Services;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Hypesoft.Application.Extensions;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        var assembly = typeof(ApplicationServiceExtensions).Assembly;

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationPipelineBehavior<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingPipelineBehavior<,>));
        });

        services.AddAutoMapper(assembly);

        services.AddValidatorsFromAssembly(assembly);

        services.AddScoped<IProductDtoEnricher, ProductDtoEnricher>();

        return services;
    }
}
