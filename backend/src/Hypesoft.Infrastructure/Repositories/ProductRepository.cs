using Hypesoft.Domain.Constants;
using Hypesoft.Domain.Entities;
using Hypesoft.Domain.Repositories;
using Hypesoft.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Hypesoft.Infrastructure.Repositories;

public sealed class ProductRepository : IProductRepository
{
    private readonly ApplicationDbContext _context;
    private readonly IMongoCollection<BsonDocument> _collection;

    public ProductRepository(ApplicationDbContext context, IMongoDatabase database)
    {
        _context = context;
        _collection = database.GetCollection<BsonDocument>("products");
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
        // When a search term is provided, delegate to the $text path which uses the
        // compound text index on { Name, Description } instead of an unindexed regex scan.
        if (!string.IsNullOrWhiteSpace(searchTerm))
            return await GetPagedByTextSearchAsync(searchTerm, categoryId, pageNumber, pageSize, cancellationToken);

        var query = _context.Products.AsQueryable();

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

    /// <summary>
    /// Uses MongoDB <c>$text</c> operator (requires the product_text_search index created by
    /// <see cref="DatabaseInitializer"/>) to perform efficient word-based search on Name and Description.
    /// Pagination is done at the database level; entities are then loaded via EF Core
    /// to ensure correct materialisation of domain types.
    /// </summary>
    private async Task<(IEnumerable<Product> Items, int TotalCount)> GetPagedByTextSearchAsync(
        string searchTerm,
        string? categoryId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var filter = Builders<BsonDocument>.Filter.Text(searchTerm, new TextSearchOptions { CaseSensitive = false });

        if (!string.IsNullOrWhiteSpace(categoryId))
            filter &= Builders<BsonDocument>.Filter.Eq("CategoryId", categoryId);

        var totalCount = (int)await _collection.CountDocumentsAsync(filter, cancellationToken: cancellationToken);

        if (totalCount == 0)
            return ([], 0);

        // Fetch only the _id values for this page, ordered by name for stable pagination
        var pageIdDocs = await _collection
            .Find(filter)
            .Sort(Builders<BsonDocument>.Sort.Ascending("Name"))
            .Skip((pageNumber - 1) * pageSize)
            .Limit(pageSize)
            .Project(Builders<BsonDocument>.Projection.Include("_id"))
            .ToListAsync(cancellationToken);

        var ids = pageIdDocs.Select(d => d["_id"].AsString).ToList();

        // Load full entities through EF Core to ensure correct domain-type materialisation
        var items = await _context.Products
            .Where(p => ids.Contains(p.Id))
            .OrderBy(p => p.Name)
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

    public async Task<IEnumerable<Product>> GetLowStockAsync(int threshold = DomainConstants.LowStockThreshold, CancellationToken cancellationToken = default)
        => await _context.Products
            .Where(p => p.StockQuantity < threshold)
            .OrderBy(p => p.StockQuantity)
            .ToListAsync(cancellationToken);

    public Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default)
        => _context.Products.CountAsync(cancellationToken);

    public async Task<decimal> GetTotalStockValueAsync(CancellationToken cancellationToken = default)
    {
        // Use native MongoDB aggregation pipeline to avoid loading all documents into memory.
        // Fields are stored in PascalCase by MongoDB.EntityFrameworkCore's default convention.
        var pipeline = new[]
        {
            new BsonDocument("$group", new BsonDocument
            {
                { "_id", BsonNull.Value },
                { "total", new BsonDocument("$sum", new BsonDocument("$multiply", new BsonArray { "$Price", "$StockQuantity" })) }
            })
        };

        var result = await _collection.Aggregate<BsonDocument>(pipeline).FirstOrDefaultAsync(cancellationToken);
        return result is null ? 0m : (decimal)result["total"].AsDecimal128;
    }

    public async Task<IEnumerable<(string CategoryId, int Count)>> GetCountByCategoryAsync(CancellationToken cancellationToken = default)
    {
        // Use native MongoDB aggregation pipeline to avoid loading all documents into memory.
        var pipeline = new[]
        {
            new BsonDocument("$group", new BsonDocument
            {
                { "_id", "$CategoryId" },
                { "count", new BsonDocument("$sum", 1) }
            })
        };

        var results = await _collection.Aggregate<BsonDocument>(pipeline).ToListAsync(cancellationToken);
        return results.Select(r => (CategoryId: r["_id"].AsString, Count: r["count"].AsInt32));
    }
}
