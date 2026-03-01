namespace Hypesoft.Application.Interfaces;

/// <summary>Centralizes cache key constants to avoid magic strings across handlers.</summary>
public static class CacheKeys
{
    public const string AllCategories = "categories_all";
    public const string DashboardSummary = "dashboard_summary";

    public static string ProductById(string id) => $"product:{id}";

    public static string ProductList(int page, int size, string? search, string? categoryId)
        => $"products:p{page}:s{size}:q{search ?? ""}:c{categoryId ?? ""}";
}
