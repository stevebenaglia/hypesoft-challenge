namespace Hypesoft.Application.DTOs;

public sealed class ProductDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public string CategoryId { get; set; } = string.Empty;
    public string? CategoryName { get; set; }
}
