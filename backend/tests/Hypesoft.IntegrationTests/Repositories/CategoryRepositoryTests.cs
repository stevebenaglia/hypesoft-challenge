using Hypesoft.Domain.Entities;
using Hypesoft.Domain.Repositories;
using Hypesoft.IntegrationTests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;

namespace Hypesoft.IntegrationTests.Repositories;

[Collection("Integration")]
public sealed class CategoryRepositoryTests : IClassFixture<HypesoftWebAppFactory>
{
    private readonly HypesoftWebAppFactory _factory;

    public CategoryRepositoryTests(HypesoftWebAppFactory factory)
    {
        _factory = factory;
    }

    private static string NewId() => ObjectId.GenerateNewId().ToString();

    private static Category BuildCategory(string id, string name)
        => Category.Create(id, name, null);

    // ── GetAllAsync ────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAllAsync_ReturnsAllStoredCategories()
    {
        using var scope = _factory.Services.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<ICategoryRepository>();
        var suffix = NewId();

        var cat1 = BuildCategory(NewId(), $"CatAll-A-{suffix}");
        var cat2 = BuildCategory(NewId(), $"CatAll-B-{suffix}");
        await repo.AddAsync(cat1);
        await repo.AddAsync(cat2);

        var all = (await repo.GetAllAsync()).ToList();

        all.Should().Contain(c => c.Id == cat1.Id);
        all.Should().Contain(c => c.Id == cat2.Id);
    }

    // ── GetByIdsAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task GetByIdsAsync_ReturnsOnlyRequestedCategories()
    {
        using var scope = _factory.Services.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<ICategoryRepository>();
        var suffix = NewId();

        var cat1 = BuildCategory(NewId(), $"ById-A-{suffix}");
        var cat2 = BuildCategory(NewId(), $"ById-B-{suffix}");
        var cat3 = BuildCategory(NewId(), $"ById-C-{suffix}");
        await repo.AddAsync(cat1);
        await repo.AddAsync(cat2);
        await repo.AddAsync(cat3);

        var results = (await repo.GetByIdsAsync([cat1.Id, cat2.Id])).ToList();

        results.Should().HaveCount(2);
        results.Select(c => c.Id).Should().BeEquivalentTo([cat1.Id, cat2.Id]);
        results.Should().NotContain(c => c.Id == cat3.Id);
    }

    [Fact]
    public async Task GetByIdsAsync_WithEmptyList_ReturnsEmpty()
    {
        using var scope = _factory.Services.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<ICategoryRepository>();

        var results = await repo.GetByIdsAsync([]);

        results.Should().BeEmpty();
    }

    // ── GetByIdAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task GetByIdAsync_ReturnsNullForNonExistentId()
    {
        using var scope = _factory.Services.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<ICategoryRepository>();

        var result = await repo.GetByIdAsync(NewId());

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsCorrectCategory()
    {
        using var scope = _factory.Services.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<ICategoryRepository>();

        var cat = BuildCategory(NewId(), $"GetById-{NewId()}");
        await repo.AddAsync(cat);

        var result = await repo.GetByIdAsync(cat.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(cat.Id);
        result.Name.Should().Be(cat.Name);
    }

    // ── ExistsByNameAsync ──────────────────────────────────────────────────────

    [Fact]
    public async Task ExistsByNameAsync_ReturnsTrueForExistingName()
    {
        using var scope = _factory.Services.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<ICategoryRepository>();
        var name = $"UniqueName-{NewId()}";

        await repo.AddAsync(BuildCategory(NewId(), name));

        var exists = await repo.ExistsByNameAsync(name);

        exists.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsByNameAsync_ReturnsFalseForNonExistentName()
    {
        using var scope = _factory.Services.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<ICategoryRepository>();

        var exists = await repo.ExistsByNameAsync($"DoesNotExist-{NewId()}");

        exists.Should().BeFalse();
    }
}
