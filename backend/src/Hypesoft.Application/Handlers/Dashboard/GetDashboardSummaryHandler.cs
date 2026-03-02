using AutoMapper;
using Hypesoft.Application.DTOs;
using Hypesoft.Application.Interfaces;
using Hypesoft.Application.Queries.Dashboard;
using Hypesoft.Domain.Repositories;
using MediatR;

namespace Hypesoft.Application.Handlers.Dashboard;

public sealed class GetDashboardSummaryHandler : IRequestHandler<GetDashboardSummaryQuery, DashboardSummaryDto>
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(2);

    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IMapper _mapper;
    private readonly ICacheService _cache;

    public GetDashboardSummaryHandler(
        IProductRepository productRepository,
        ICategoryRepository categoryRepository,
        IMapper mapper,
        ICacheService cache)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _mapper = mapper;
        _cache = cache;
    }

    public async Task<DashboardSummaryDto> Handle(GetDashboardSummaryQuery request, CancellationToken cancellationToken)
    {
        var cached = await _cache.GetAsync<DashboardSummaryDto>(CacheKeys.DashboardSummary, cancellationToken);
        if (cached is not null)
            return cached;

        var totalCountTask = _productRepository.GetTotalCountAsync(cancellationToken);
        var totalValueTask = _productRepository.GetTotalStockValueAsync(cancellationToken);
        var countByCategoryTask = _productRepository.GetCountByCategoryAsync(cancellationToken);
        var lowStockTask = _productRepository.GetLowStockAsync(cancellationToken: cancellationToken);
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

        var summary = new DashboardSummaryDto
        {
            TotalProducts = await totalCountTask,
            TotalStockValue = await totalValueTask,
            LowStockProducts = lowStockDtos,
            ProductsByCategory = productsByCategory
        };

        await _cache.SetAsync(CacheKeys.DashboardSummary, summary, CacheDuration, cancellationToken);

        return summary;
    }
}
