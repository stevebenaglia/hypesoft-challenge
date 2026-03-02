using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MongoDB.EntityFrameworkCore.Extensions;
using Testcontainers.MongoDb;

namespace Hypesoft.IntegrationTests.Infrastructure;

/// <summary>
/// Integration test factory. Starts a MongoDB Testcontainer and replaces production
/// auth with a simple test scheme that reads roles from the "X-Test-Roles" header.
/// </summary>
public sealed class HypesoftWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly MongoDbContainer _mongoContainer = new MongoDbBuilder()
        .WithImage("mongo:7")
        .Build();

    public async Task InitializeAsync()
    {
        await _mongoContainer.StartAsync();
    }

    public new async Task DisposeAsync()
    {
        await _mongoContainer.StopAsync();
        await base.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // ── Replace MongoDB DbContext with Testcontainer instance ──
            var dbContextDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<Hypesoft.Infrastructure.Data.ApplicationDbContext>));
            if (dbContextDescriptor is not null)
                services.Remove(dbContextDescriptor);

            services.AddDbContext<Hypesoft.Infrastructure.Data.ApplicationDbContext>(options =>
                options.UseMongoDB(_mongoContainer.GetConnectionString(), "hypesoft_test"));

            // ── Replace JWT authentication with TestAuthHandler ──
            services.RemoveAll<IAuthenticationSchemeProvider>();
            services.AddAuthentication(TestAuthHandler.SchemeName)
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.SchemeName, _ => { });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminOnly", policy => policy.RequireRole("admin"));
            });
        });

        builder.UseEnvironment("Testing");
    }

    /// <summary>Creates an HTTP client that sends requests as an admin user.</summary>
    public HttpClient CreateAdminClient()
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.RolesHeader, "admin,user");
        return client;
    }

    /// <summary>Creates an HTTP client that sends requests as a regular user (no admin role).</summary>
    public HttpClient CreateUserClient()
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.RolesHeader, "user");
        return client;
    }

    /// <summary>Creates an unauthenticated HTTP client.</summary>
    public HttpClient CreateAnonymousClient() => CreateClient();
}
