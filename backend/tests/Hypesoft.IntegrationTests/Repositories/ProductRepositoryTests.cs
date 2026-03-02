using Hypesoft.Domain.Constants;
using Hypesoft.Domain.Entities;
using Hypesoft.Domain.Repositories;
using Hypesoft.Domain.ValueObjects;
using Hypesoft.IntegrationTests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;

namespace Hypesoft.IntegrationTests.Repositories;

/// <summary>
/// Tests the ProductRepository query logic against a real MongoDB Testcontainer.
/// Each test method seeds its own uniquely-identified data to avoid cross-test interference.
/// </summary>
[Collection("Integration")]
public sealed class ProductRepositoryTests : IClassFixture<HypesoftWebAppFactory>
{
    private readonly HypesoftWebAppFactory _factory;

    public ProductRepositoryTests(HypesoftWebAppFactory factory)
    {
        _factory = factory;
    }

    private static string NewId() => ObjectId.GenerateNewId().ToString();

    private static Product BuildProduct(string id, string name, decimal price, int stock, string categoryId)
        => Product.Create(id, ProductName.Create(name), null, Money.Create(price), StockQuantity.Create(stock), categoryId);

    // ── GetPagedAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task GetPagedAsync_DefaultPagination_ReturnsTotalCountAndItems()
    {
        using var scope = _factory.Services.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IProductRepository>();
        var catId = NewId();

        var p1 = BuildProduct(NewId(), $"Paginate-A-{NewId()}", 10m, 5, catId);
        var p2 = BuildProduct(NewId(), $"Paginate-B-{NewId()}", 20m, 15, catId);
        await repo.AddAsync(p1);
        await repo.AddAsync(p2);

        var (items, totalCount) = await repo.GetPagedAsync(1, 100, searchTerm: null, categoryId: catId);

        totalCount.Should().Be(2);
        items.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetPagedAsync_SearchTerm_ReturnsOnlyMatchingProducts()
    {
        using var scope = _factory.Services.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IProductRepository>();
        var catId = NewId();

        // Use full words so $text search tokenises them correctly.
        await repo.AddAsync(BuildProduct(NewId(), "Keyboard Wireless", 10m, 5, catId));
        await repo.AddAsync(BuildProduct(NewId(), "Monitor Ultrawide", 10m, 5, catId));

        var (items, totalCount) = await repo.GetPagedAsync(1, 10, searchTerm: "Keyboard", categoryId: catId);

        totalCount.Should().Be(1);
        items.Single().Name.Should().Contain("Keyboard");
    }

    [Fact]
    public async Task GetPagedAsync_CategoryFilter_ReturnsOnlyProductsInThatCategory()
    {
        using var scope = _factory.Services.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IProductRepository>();
        var targetCatId = NewId();
        var otherCatId = NewId();

        await repo.AddAsync(BuildProduct(NewId(), "Tablet Pro Target", 10m, 5, targetCatId));
        await repo.AddAsync(BuildProduct(NewId(), "Tablet Pro Other", 10m, 5, otherCatId));

        // Isolate by categoryId; no searchTerm needed for this test.
        var (items, totalCount) = await repo.GetPagedAsync(1, 10, searchTerm: null, categoryId: targetCatId);

        totalCount.Should().Be(1);
        items.Single().CategoryId.Should().Be(targetCatId);
    }

    [Fact]
    public async Task GetPagedAsync_Pagination_RespectsPageSizeAndSkip()
    {
        using var scope = _factory.Services.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IProductRepository>();
        // A unique categoryId isolates this test's data without relying on searchTerm.
        var catId = NewId();

        for (var i = 1; i <= 5; i++)
            await repo.AddAsync(BuildProduct(NewId(), $"PaginationItem {i:D2}", 10m, 5, catId));

        var (page1Items, total) = await repo.GetPagedAsync(1, 2, searchTerm: null, categoryId: catId);
        var (page2Items, _) = await repo.GetPagedAsync(2, 2, searchTerm: null, categoryId: catId);

        total.Should().Be(5);
        page1Items.Should().HaveCount(2);
        page2Items.Should().HaveCount(2);
        page1Items.Select(p => p.Name).Should().NotIntersectWith(page2Items.Select(p => p.Name));
    }

    // ── GetLowStockAsync ───────────────────────────────────────────────────────

    [Fact]
    public async Task GetLowStockAsync_ReturnsOnlyProductsBelowThreshold()
    {
        using var scope = _factory.Services.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IProductRepository>();
        var catId = NewId();

        var lowStock = BuildProduct(NewId(), $"LowStock-{NewId()}", 10m, DomainConstants.LowStockThreshold - 1, catId);
        var normalStock = BuildProduct(NewId(), $"NormalStock-{NewId()}", 10m, DomainConstants.LowStockThreshold, catId);
        await repo.AddAsync(lowStock);
        await repo.AddAsync(normalStock);

        var results = (await repo.GetLowStockAsync()).ToList();

        results.Should().Contain(p => p.Id == lowStock.Id);
        results.Should().NotContain(p => p.Id == normalStock.Id);
    }

    // ── GetTotalStockValueAsync ────────────────────────────────────────────────

    [Fact]
    public async Task GetTotalStockValueAsync_ReturnsSumOfPriceTimesStock()
    {
        using var scope = _factory.Services.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IProductRepository>();
        var catId = NewId();

        // Use a unique category so we can isolate the products for this test.
        // Unfortunately GetTotalStockValueAsync sums ALL products, so we rely on
        // the result being >= our expected contribution.
        var p1 = BuildProduct(NewId(), $"ValueTest-A-{NewId()}", 5m, 4, catId);   // 5 * 4 = 20
        var p2 = BuildProduct(NewId(), $"ValueTest-B-{NewId()}", 10m, 3, catId);  // 10 * 3 = 30
        await repo.AddAsync(p1);
        await repo.AddAsync(p2);

        var total = await repo.GetTotalStockValueAsync();

        // Total stock value across all products must be at least 50 (our contribution).
        total.Should().BeGreaterThanOrEqualTo(50m);
    }

    // ── GetCountByCategoryAsync ────────────────────────────────────────────────

    [Fact]
    public async Task GetCountByCategoryAsync_GroupsProductsByCategory()
    {
        using var scope = _factory.Services.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IProductRepository>();
        var catA = NewId();
        var catB = NewId();

        await repo.AddAsync(BuildProduct(NewId(), $"CatA-1-{NewId()}", 10m, 5, catA));
        await repo.AddAsync(BuildProduct(NewId(), $"CatA-2-{NewId()}", 10m, 5, catA));
        await repo.AddAsync(BuildProduct(NewId(), $"CatB-1-{NewId()}", 10m, 5, catB));

        var groups = (await repo.GetCountByCategoryAsync()).ToList();

        groups.Should().Contain(g => g.CategoryId == catA && g.Count >= 2);
        groups.Should().Contain(g => g.CategoryId == catB && g.Count >= 1);
    }

    // ── ExistsAsync / HasProductsInCategoryAsync ───────────────────────────────

    [Fact]
    public async Task ExistsAsync_ReturnsFalseForNonExistentProduct()
    {
        using var scope = _factory.Services.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IProductRepository>();

        var exists = await repo.ExistsAsync(NewId());

        exists.Should().BeFalse();
    }

    [Fact]
    public async Task HasProductsInCategoryAsync_ReturnsTrueWhenProductsExist()
    {
        using var scope = _factory.Services.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IProductRepository>();
        var catId = NewId();
        await repo.AddAsync(BuildProduct(NewId(), $"HasProd-{NewId()}", 10m, 5, catId));

        var hasProducts = await repo.HasProductsInCategoryAsync(catId);

        hasProducts.Should().BeTrue();
    }

    [Fact]
    public async Task HasProductsInCategoryAsync_ReturnsFalseForEmptyCategory()
    {
        using var scope = _factory.Services.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IProductRepository>();

        var hasProducts = await repo.HasProductsInCategoryAsync(NewId());

        hasProducts.Should().BeFalse();
    }
}
