using Hypesoft.Application.Interfaces;

namespace Hypesoft.Infrastructure.Services;

/// <summary>
/// Centralises cache invalidation logic so mutation handlers do not need to
/// know which cache keys to remove. Uses a generation counter to effectively
/// invalidate all paginated product-list entries without requiring
/// pattern-based deletion (which is not supported by IDistributedCache).
/// </summary>
public sealed class CacheInvalidationService : ICacheInvalidationService
{
    private readonly ICacheService _cache;

    public CacheInvalidationService(ICacheService cache)
    {
        _cache = cache;
    }

    public async Task InvalidateProductMutationAsync(string? productId, CancellationToken cancellationToken = default)
    {
        var tasks = new List<Task>
        {
            _cache.RemoveAsync(CacheKeys.DashboardSummary, cancellationToken),
            BumpProductListGenerationAsync(cancellationToken)
        };

        if (productId is not null)
            tasks.Add(_cache.RemoveAsync(CacheKeys.ProductById(productId), cancellationToken));

        await Task.WhenAll(tasks);
    }

    public Task InvalidateCategoryMutationAsync(CancellationToken cancellationToken = default)
        => _cache.RemoveAsync(CacheKeys.AllCategories, cancellationToken);

    public async Task<int> GetProductListGenerationAsync(CancellationToken cancellationToken = default)
    {
        // GetAsync<int> returns default(int) = 0 when the key does not exist.
        var gen = await _cache.GetAsync<int>(CacheKeys.ProductListGeneration, cancellationToken);
        return gen;
    }

    private async Task BumpProductListGenerationAsync(CancellationToken cancellationToken)
    {
        var current = await _cache.GetAsync<int>(CacheKeys.ProductListGeneration, cancellationToken);
        await _cache.SetAsync(CacheKeys.ProductListGeneration, current + 1, TimeSpan.FromHours(24), cancellationToken);
    }
}
