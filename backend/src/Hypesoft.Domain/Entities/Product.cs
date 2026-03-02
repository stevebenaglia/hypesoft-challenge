using Hypesoft.Domain.Constants;
using Hypesoft.Domain.ValueObjects;

namespace Hypesoft.Domain.Entities;

public sealed class Product
{
    public string Id { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public decimal Price { get; private set; }
    public int StockQuantity { get; private set; }
    public string CategoryId { get; private set; } = string.Empty;

    /// <summary>Required by EF Core for materialization.</summary>
    private Product() { }

    public static Product Create(string id, ProductName name, string? description, Money price, StockQuantity stockQuantity, string categoryId)
    {
        return new Product
        {
            Id = id,
            Name = name.Value,
            Description = description,
            Price = price.Amount,
            StockQuantity = stockQuantity.Value,
            CategoryId = categoryId
        };
    }

    public void Update(ProductName name, string? description, Money price, StockQuantity stockQuantity, string categoryId)
    {
        Name = name.Value;
        Description = description;
        Price = price.Amount;
        StockQuantity = stockQuantity.Value;
        CategoryId = categoryId;
    }

    public void UpdateStock(StockQuantity quantity)
    {
        StockQuantity = quantity.Value;
    }

    public bool IsLowStock() => StockQuantity < DomainConstants.LowStockThreshold;
}
