using MongoDB.Bson;
using MongoDB.Driver;

namespace Hypesoft.Infrastructure.Data;

/// <summary>
/// Creates MongoDB indexes that cannot be expressed through the EF Core model (e.g. text indexes).
/// Safe to call on every startup — MongoDB ignores duplicate requests for existing indexes.
/// </summary>
public static class DatabaseInitializer
{
    public static async Task EnsureIndexesAsync(IMongoDatabase database, CancellationToken cancellationToken = default)
    {
        var products = database.GetCollection<BsonDocument>("products");

        // Compound text index on Name + Description powers the $text operator used in
        // ProductRepository.GetPagedAsync when a searchTerm is provided.
        // Word-based tokenisation is much faster than a collection-wide regex scan.
        var textKeys = Builders<BsonDocument>.IndexKeys
            .Text("Name")
            .Text("Description");

        await products.Indexes.CreateOneAsync(
            new CreateIndexModel<BsonDocument>(
                textKeys,
                new CreateIndexOptions { Name = "product_text_search" }),
            cancellationToken: cancellationToken);
    }
}
