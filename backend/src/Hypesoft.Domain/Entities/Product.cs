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

    public static Product Create(string id, string name, string? description, decimal price, int stockQuantity, string categoryId)
    {
        return new Product
        {
            Id = id,
            Name = name,
            Description = description,
            Price = price,
            StockQuantity = stockQuantity,
            CategoryId = categoryId
        };
    }

    public void Update(string name, string? description, decimal price, int stockQuantity, string categoryId)
    {
        Name = name;
        Description = description;
        Price = price;
        StockQuantity = stockQuantity;
        CategoryId = categoryId;
    }

    public bool IsLowStock() => StockQuantity < 10;
}
