using System.Text.Json;
using Hypesoft.IntegrationTests.Infrastructure;

namespace Hypesoft.IntegrationTests.Controllers;

[Collection("Integration")]
public sealed class DashboardControllerTests : IClassFixture<HypesoftWebAppFactory>
{
    private readonly HypesoftWebAppFactory _factory;

    public DashboardControllerTests(HypesoftWebAppFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetSummary_WithAuth_ShouldReturn200WithExpectedShape()
    {
        var client = _factory.CreateUserClient();

        var response = await client.GetAsync("/api/dashboard");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadFromJsonAsync<JsonElement>();
        content.TryGetProperty("totalProducts", out _).Should().BeTrue();
        content.TryGetProperty("totalStockValue", out _).Should().BeTrue();
        content.TryGetProperty("lowStockProducts", out _).Should().BeTrue();
        content.TryGetProperty("productsByCategory", out _).Should().BeTrue();
    }

    [Fact]
    public async Task GetSummary_WithoutAuth_ShouldReturn401()
    {
        var client = _factory.CreateAnonymousClient();

        var response = await client.GetAsync("/api/dashboard");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
