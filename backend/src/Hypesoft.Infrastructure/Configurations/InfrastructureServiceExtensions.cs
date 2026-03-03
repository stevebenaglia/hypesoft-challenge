using Hypesoft.Application.Interfaces;
using Hypesoft.Domain.Repositories;
using Hypesoft.Domain.Services;
using Hypesoft.Infrastructure.Data;
using Hypesoft.Infrastructure.Repositories;
using Hypesoft.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Driver;

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

        // Store decimal as Decimal128 (BSON numeric type) instead of the default string representation.
        // TryRegisterSerializer is used to avoid duplicate-registration errors in test hosts.
        MongoDB.Bson.Serialization.BsonSerializer.TryRegisterSerializer(
            new MongoDB.Bson.Serialization.Serializers.DecimalSerializer(BsonType.Decimal128));

        var mongoClient = new MongoClient(connectionString);
        services.AddSingleton<IMongoClient>(mongoClient);
        services.AddSingleton<IMongoDatabase>(mongoClient.GetDatabase(databaseName));

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseMongoDB(connectionString, databaseName));

        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddSingleton<IIdGenerator, ObjectIdGenerator>();

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        services.AddDistributedMemoryCache();
        services.AddScoped<ICacheService, DistributedCacheService>();
        services.AddScoped<ICacheInvalidationService, CacheInvalidationService>();

        return services;
    }
}
