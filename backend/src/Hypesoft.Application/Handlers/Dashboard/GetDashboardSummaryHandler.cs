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
        var totalCountTask = _productRepository.GetTotalCountAsync(cancellationToken);
        var totalValueTask = _productRepository.GetTotalStockValueAsync(cancellationToken);
        var countByCategoryTask = _productRepository.GetCountByCategoryAsync(cancellationToken);
        var lowStockTask = _productRepository.GetLowStockAsync(10, cancellationToken);
        var categoriesTask = _categoryRepository.GetAllAsync(cancellationToken);

        await Task.WhenAll(totalCountTask, totalValueTask, countByCategoryTask, lowStockTask, categoriesTask);

        var categoryMap = (await categoriesTask).ToDictionary(c => c.Id, c => c.Name);

        var lowStockDtos = _mapper.Map<IEnumerable<ProductDto>>(await lowStockTask).ToList();
        foreach (var dto in lowStockDtos)
            dto.CategoryName = categoryMap.GetValueOrDefault(dto.CategoryId);

        var productsByCategory = (await countByCategoryTask)
            .Select(g => new CategorySummaryDto
            {
                CategoryName = categoryMap.GetValueOrDefault(g.CategoryId) ?? g.CategoryId,
                ProductCount = g.Count
            })
            .OrderByDescending(x => x.ProductCount)
            .ToList();

        return new DashboardSummaryDto
        {
            TotalProducts = await totalCountTask,
            TotalStockValue = await totalValueTask,
            LowStockProducts = lowStockDtos,
            ProductsByCategory = productsByCategory
        };
    }
}
