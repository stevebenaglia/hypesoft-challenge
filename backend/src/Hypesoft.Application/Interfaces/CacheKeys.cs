namespace Hypesoft.Application.Interfaces;

/// <summary>Centralizes cache key constants to avoid magic strings across handlers.</summary>
public static class CacheKeys
{
    public const string AllCategories = "categories_all";
    public const string DashboardSummary = "dashboard_summary";

    /// <summary>
    /// Monotonically increasing counter stored in cache.
    /// Bumped on every product mutation so all paginated product-list entries
    /// become unreachable without requiring pattern-based key deletion.
    /// </summary>
    public const string ProductListGeneration = "products:generation";

    public static string ProductById(string id) => $"product:{id}";

    /// <summary>
    /// Builds a product-list cache key scoped to the current generation.
    /// Pass the value returned by <see cref="ICacheInvalidationService.GetProductListGenerationAsync"/>.
    /// </summary>
    public static string ProductList(int generation, int page, int size, string? search, string? categoryId, bool lowStockOnly = false)
        => $"products:g{generation}:p{page}:s{size}:q{search ?? ""}:c{categoryId ?? ""}:ls{(lowStockOnly ? 1 : 0)}";
}
