using Hypesoft.Domain.Repositories;
using Hypesoft.Domain.Services;
using Hypesoft.Infrastructure.Data;
using Hypesoft.Infrastructure.Repositories;
using Hypesoft.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Hypesoft.Infrastructure.Configurations;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration["MongoDB:ConnectionString"]
            ?? throw new InvalidOperationException("MongoDB:ConnectionString is not configured.");
        var databaseName = configuration["MongoDB:DatabaseName"]
            ?? throw new InvalidOperationException("MongoDB:DatabaseName is not configured.");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseMongoDB(connectionString, databaseName));

        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddSingleton<IIdGenerator, ObjectIdGenerator>();

        return services;
    }
}
