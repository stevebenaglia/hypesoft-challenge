namespace Hypesoft.Application.DTOs;

public sealed class DashboardSummaryDto
{
    public int TotalProducts { get; set; }
    public decimal TotalStockValue { get; set; }
    public IEnumerable<ProductDto> LowStockProducts { get; set; } = [];
    public IEnumerable<CategorySummaryDto> ProductsByCategory { get; set; } = [];
}
