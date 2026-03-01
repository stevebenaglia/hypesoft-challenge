using Hypesoft.Domain.Entities;
using Hypesoft.Domain.Repositories;
using Hypesoft.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Hypesoft.Infrastructure.Repositories;

public sealed class ProductRepository : IProductRepository
{
    private readonly ApplicationDbContext _context;

    public ProductRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Product?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
        => await _context.Products
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task<(IEnumerable<Product> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        string? searchTerm,
        string? categoryId,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Products.AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
            query = query.Where(p => p.Name.ToLower().Contains(searchTerm.ToLower()));

        if (!string.IsNullOrWhiteSpace(categoryId))
            query = query.Where(p => p.CategoryId == categoryId);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(p => p.Name)  // Consistent ordering prevents unstable pagination across pages
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<Product> AddAsync(Product product, CancellationToken cancellationToken = default)
    {
        await _context.Products.AddAsync(product, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return product;
    }

    public async Task UpdateAsync(Product product, CancellationToken cancellationToken = default)
    {
        _context.Products.Update(product);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Product product, CancellationToken cancellationToken = default)
    {
        _context.Products.Remove(product);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(string id, CancellationToken cancellationToken = default)
        => await _context.Products.AnyAsync(p => p.Id == id, cancellationToken);

    public async Task<bool> HasProductsInCategoryAsync(string categoryId, CancellationToken cancellationToken = default)
        => await _context.Products.AnyAsync(p => p.CategoryId == categoryId, cancellationToken);

    public async Task<IEnumerable<Product>> GetLowStockAsync(int threshold = 10, CancellationToken cancellationToken = default)
        => await _context.Products
            .Where(p => p.StockQuantity < threshold)
            .OrderBy(p => p.StockQuantity)
            .ToListAsync(cancellationToken);

    public Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default)
        => _context.Products.CountAsync(cancellationToken);

    public async Task<decimal> GetTotalStockValueAsync(CancellationToken cancellationToken = default)
    {
        // MongoDB.EntityFrameworkCore 8.x does not support SumAsync with a selector expression.
        // Project only the needed fields and compute the sum in memory.
        var rows = await _context.Products
            .Select(p => new { p.Price, p.StockQuantity })
            .ToListAsync(cancellationToken);
        return rows.Sum(p => p.Price * p.StockQuantity);
    }

    public async Task<IEnumerable<(string CategoryId, int Count)>> GetCountByCategoryAsync(CancellationToken cancellationToken = default)
    {
        var groups = await _context.Products
            .GroupBy(p => p.CategoryId)
            .Select(g => new { CategoryId = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        return groups.Select(g => (g.CategoryId, g.Count));
    }
}
