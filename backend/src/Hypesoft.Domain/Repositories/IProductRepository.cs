using Hypesoft.Domain.Entities;

namespace Hypesoft.Domain.Repositories;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(string id, CancellationToken cancellationToken = default);

    /// <returns>Matched products and total count before pagination.</returns>
    Task<(IEnumerable<Product> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        string? searchTerm,
        string? categoryId,
        CancellationToken cancellationToken = default);

    Task<Product> AddAsync(Product product, CancellationToken cancellationToken = default);
    Task UpdateAsync(Product product, CancellationToken cancellationToken = default);
    Task DeleteAsync(Product product, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>Returns true if at least one product is associated with the given category.</summary>
    Task<bool> HasProductsInCategoryAsync(string categoryId, CancellationToken cancellationToken = default);
}
