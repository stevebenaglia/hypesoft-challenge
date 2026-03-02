using System.Text.Json;
using Hypesoft.IntegrationTests.Infrastructure;

namespace Hypesoft.IntegrationTests.Controllers;

[Collection("Integration")]
public sealed class ProductsControllerTests : IClassFixture<HypesoftWebAppFactory>
{
    private readonly HypesoftWebAppFactory _factory;
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    public ProductsControllerTests(HypesoftWebAppFactory factory)
    {
        _factory = factory;
    }

    /// <summary>Creates a category and returns its ID (helper).</summary>
    private async Task<string> CreateCategoryAsync(string name = "Test-Category")
    {
        var client = _factory.CreateAdminClient();
        var response = await client.PostAsJsonAsync("/api/categories", new { name });
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        return json.GetProperty("id").GetString()!;
    }

    // ────────────────────── GET /api/products ──────────────────────

    [Fact]
    public async Task GetAll_WithAuth_ShouldReturn200()
    {
        var client = _factory.CreateUserClient();

        var response = await client.GetAsync("/api/products");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetAll_WithoutAuth_ShouldReturn401()
    {
        var client = _factory.CreateAnonymousClient();

        var response = await client.GetAsync("/api/products");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ────────────────────── POST /api/products ──────────────────────

    [Fact]
    public async Task Create_WithAdminRole_ShouldReturn201()
    {
        var catId = await CreateCategoryAsync("Prod-Create-Cat");
        var client = _factory.CreateAdminClient();
        var body = new { name = "Test Laptop", price = 2999.99m, stockQuantity = 10, categoryId = catId };

        var response = await client.PostAsJsonAsync("/api/products", body);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task Create_WithUserRole_ShouldReturn403()
    {
        var client = _factory.CreateUserClient();
        var body = new { name = "Test Laptop", price = 2999.99m, stockQuantity = 10, categoryId = "any" };

        var response = await client.PostAsJsonAsync("/api/products", body);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Create_WithoutAuth_ShouldReturn401()
    {
        var client = _factory.CreateAnonymousClient();
        var body = new { name = "Test Laptop", price = 2999.99m, stockQuantity = 10, categoryId = "any" };

        var response = await client.PostAsJsonAsync("/api/products", body);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Create_WithInvalidPrice_ShouldReturn422()
    {
        var catId = await CreateCategoryAsync("Prod-Invalid-Price-Cat");
        var client = _factory.CreateAdminClient();
        var body = new { name = "Test Laptop", price = 0m, stockQuantity = 10, categoryId = catId };

        var response = await client.PostAsJsonAsync("/api/products", body);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    // ────────────────────── GET /api/products/{id} ──────────────────────

    [Fact]
    public async Task GetById_NonExistentId_ShouldReturn404()
    {
        var client = _factory.CreateUserClient();

        var response = await client.GetAsync("/api/products/nonexistent-prod-99999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetById_AfterCreate_ShouldReturn200()
    {
        var catId = await CreateCategoryAsync("Prod-GetById-Cat");
        var client = _factory.CreateAdminClient();
        var createBody = new { name = "Get-Product", price = 99.99m, stockQuantity = 5, categoryId = catId };
        var created = await client.PostAsJsonAsync("/api/products", createBody);
        var json = await created.Content.ReadFromJsonAsync<JsonElement>();
        var id = json.GetProperty("id").GetString()!;

        var response = await client.GetAsync($"/api/products/{id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // ────────────────────── PATCH /api/products/{id}/stock ──────────────────────

    [Fact]
    public async Task UpdateStock_WithAdminRole_ShouldReturn200()
    {
        var catId = await CreateCategoryAsync("Prod-Stock-Cat");
        var client = _factory.CreateAdminClient();
        var createBody = new { name = "Stock-Product", price = 99.99m, stockQuantity = 5, categoryId = catId };
        var created = await client.PostAsJsonAsync("/api/products", createBody);
        var json = await created.Content.ReadFromJsonAsync<JsonElement>();
        var id = json.GetProperty("id").GetString()!;

        var response = await client.PatchAsJsonAsync($"/api/products/{id}/stock", new { quantity = 100 });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task UpdateStock_WithNegativeQuantity_ShouldReturn422()
    {
        var catId = await CreateCategoryAsync("Prod-Stock-Invalid-Cat");
        var client = _factory.CreateAdminClient();
        var createBody = new { name = "Stock-Invalid-Product", price = 49.99m, stockQuantity = 5, categoryId = catId };
        var created = await client.PostAsJsonAsync("/api/products", createBody);
        var json = await created.Content.ReadFromJsonAsync<JsonElement>();
        var id = json.GetProperty("id").GetString()!;

        var response = await client.PatchAsJsonAsync($"/api/products/{id}/stock", new { quantity = -1 });

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    // ────────────────────── DELETE /api/products/{id} ──────────────────────

    [Fact]
    public async Task Delete_WithAdminRole_ShouldReturn204()
    {
        var catId = await CreateCategoryAsync("Prod-Delete-Cat");
        var client = _factory.CreateAdminClient();
        var createBody = new { name = "Delete-Product", price = 49.99m, stockQuantity = 5, categoryId = catId };
        var created = await client.PostAsJsonAsync("/api/products", createBody);
        var json = await created.Content.ReadFromJsonAsync<JsonElement>();
        var id = json.GetProperty("id").GetString()!;

        var response = await client.DeleteAsync($"/api/products/{id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Delete_WithUserRole_ShouldReturn403()
    {
        var client = _factory.CreateUserClient();

        var response = await client.DeleteAsync("/api/products/any-id");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
