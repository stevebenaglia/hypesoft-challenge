using System.Text.Json;
using Hypesoft.IntegrationTests.Infrastructure;

namespace Hypesoft.IntegrationTests.Controllers;

[Collection("Integration")]
public sealed class CategoriesControllerTests : IClassFixture<HypesoftWebAppFactory>
{
    private readonly HypesoftWebAppFactory _factory;
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    public CategoriesControllerTests(HypesoftWebAppFactory factory)
    {
        _factory = factory;
    }

    // ────────────────────── GET /api/categories ──────────────────────

    [Fact]
    public async Task GetAll_WithAuthenticatedUser_ShouldReturn200()
    {
        var client = _factory.CreateUserClient();

        var response = await client.GetAsync("/api/categories");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetAll_WithoutAuth_ShouldReturn401()
    {
        var client = _factory.CreateAnonymousClient();

        var response = await client.GetAsync("/api/categories");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ────────────────────── POST /api/categories ──────────────────────

    [Fact]
    public async Task Create_WithAdminRole_ShouldReturn201()
    {
        var client = _factory.CreateAdminClient();
        var body = new { name = "Electronics", description = "All electronic goods" };

        var response = await client.PostAsJsonAsync("/api/categories", body);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task Create_WithUserRole_ShouldReturn403()
    {
        var client = _factory.CreateUserClient();
        var body = new { name = "TestCategory" };

        var response = await client.PostAsJsonAsync("/api/categories", body);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Create_WithoutAuth_ShouldReturn401()
    {
        var client = _factory.CreateAnonymousClient();
        var body = new { name = "TestCategory" };

        var response = await client.PostAsJsonAsync("/api/categories", body);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Create_WithEmptyName_ShouldReturn422()
    {
        var client = _factory.CreateAdminClient();
        var body = new { name = "" };

        var response = await client.PostAsJsonAsync("/api/categories", body);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    // ────────────────────── GET /api/categories/{id} ──────────────────────

    [Fact]
    public async Task GetById_NonExistentId_ShouldReturn404()
    {
        var client = _factory.CreateUserClient();

        var response = await client.GetAsync("/api/categories/nonexistent-id-12345");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetById_AfterCreate_ShouldReturn200WithCorrectData()
    {
        var client = _factory.CreateAdminClient();
        var body = new { name = "GetById-Test" };

        var created = await client.PostAsJsonAsync("/api/categories", body);
        created.StatusCode.Should().Be(HttpStatusCode.Created);

        var createdContent = await created.Content.ReadFromJsonAsync<JsonElement>();
        var id = createdContent.GetProperty("id").GetString()!;

        var response = await client.GetAsync($"/api/categories/{id}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadFromJsonAsync<JsonElement>();
        content.GetProperty("name").GetString().Should().Be("GetById-Test");
    }

    // ────────────────────── PUT /api/categories/{id} ──────────────────────

    [Fact]
    public async Task Update_WithAdminRole_ShouldReturn200()
    {
        var client = _factory.CreateAdminClient();
        var createBody = new { name = "Update-Original" };
        var created = await client.PostAsJsonAsync("/api/categories", createBody);
        var createdContent = await created.Content.ReadFromJsonAsync<JsonElement>();
        var id = createdContent.GetProperty("id").GetString()!;

        var updateBody = new { name = "Update-Modified", description = "Updated" };
        var response = await client.PutAsJsonAsync($"/api/categories/{id}", updateBody);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Update_NonExistentId_ShouldReturn404()
    {
        var client = _factory.CreateAdminClient();
        var body = new { name = "Doesn't matter" };

        var response = await client.PutAsJsonAsync("/api/categories/nonexistent-999", body);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ────────────────────── DELETE /api/categories/{id} ──────────────────────

    [Fact]
    public async Task Delete_WithAdminRole_NoProducts_ShouldReturn204()
    {
        var client = _factory.CreateAdminClient();
        var createBody = new { name = "Delete-Category" };
        var created = await client.PostAsJsonAsync("/api/categories", createBody);
        var createdContent = await created.Content.ReadFromJsonAsync<JsonElement>();
        var id = createdContent.GetProperty("id").GetString()!;

        var response = await client.DeleteAsync($"/api/categories/{id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Delete_WithUserRole_ShouldReturn403()
    {
        var client = _factory.CreateUserClient();

        var response = await client.DeleteAsync("/api/categories/any-id");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
