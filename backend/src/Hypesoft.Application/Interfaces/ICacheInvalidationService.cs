namespace Hypesoft.Application.Interfaces;

public interface ICacheInvalidationService
{
    /// <summary>
    /// Invalidates all caches affected by a product mutation.
    /// Removes the product-by-ID entry, the dashboard summary, and bumps the
    /// product-list generation so existing paginated results are no longer served.
    /// </summary>
    Task InvalidateProductMutationAsync(string? productId, CancellationToken cancellationToken = default);

    /// <summary>Invalidates the categories list cache.</summary>
    Task InvalidateCategoryMutationAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the current product-list cache generation.
    /// Used by read handlers to build a generation-scoped cache key.
    /// </summary>
    Task<int> GetProductListGenerationAsync(CancellationToken cancellationToken = default);
}
