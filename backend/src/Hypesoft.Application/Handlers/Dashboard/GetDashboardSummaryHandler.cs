using AutoMapper;
using Hypesoft.Application.DTOs;
using Hypesoft.Application.Queries.Dashboard;
using Hypesoft.Domain.Repositories;
using MediatR;

namespace Hypesoft.Application.Handlers.Dashboard;

public sealed class GetDashboardSummaryHandler : IRequestHandler<GetDashboardSummaryQuery, DashboardSummaryDto>
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IMapper _mapper;

    public GetDashboardSummaryHandler(
        IProductRepository productRepository,
        ICategoryRepository categoryRepository,
        IMapper mapper)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _mapper = mapper;
    }

    public async Task<DashboardSummaryDto> Handle(GetDashboardSummaryQuery request, CancellationToken cancellationToken)
    {
        var (allProducts, totalCount) = await _productRepository.GetPagedAsync(1, int.MaxValue, null, null, cancellationToken);
        var productList = allProducts.ToList();

        var categories = (await _categoryRepository.GetAllAsync(cancellationToken)).ToList();
        var categoryMap = categories.ToDictionary(c => c.Id, c => c.Name);

        var totalStockValue = productList.Sum(p => p.Price * p.StockQuantity);

        var lowStockProducts = await _productRepository.GetLowStockAsync(10, cancellationToken);
        var lowStockDtos = _mapper.Map<IEnumerable<ProductDto>>(lowStockProducts).ToList();
        foreach (var dto in lowStockDtos)
            dto.CategoryName = categoryMap.GetValueOrDefault(dto.CategoryId);

        var productsByCategory = productList
            .GroupBy(p => p.CategoryId)
            .Select(g => new CategorySummaryDto
            {
                CategoryName = categoryMap.GetValueOrDefault(g.Key) ?? g.Key,
                ProductCount = g.Count()
            })
            .OrderByDescending(x => x.ProductCount)
            .ToList();

        return new DashboardSummaryDto
        {
            TotalProducts = totalCount,
            TotalStockValue = totalStockValue,
            LowStockProducts = lowStockDtos,
            ProductsByCategory = productsByCategory
        };
    }
}
