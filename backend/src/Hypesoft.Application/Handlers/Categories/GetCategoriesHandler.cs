using AutoMapper;
using Hypesoft.Application.DTOs;
using Hypesoft.Application.Interfaces;
using Hypesoft.Application.Queries.Categories;
using Hypesoft.Domain.Repositories;
using MediatR;

namespace Hypesoft.Application.Handlers.Categories;

public sealed class GetCategoriesHandler : IRequestHandler<GetCategoriesQuery, IEnumerable<CategoryDto>>
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

    private readonly ICategoryRepository _categoryRepository;
    private readonly IProductRepository _productRepository;
    private readonly IMapper _mapper;
    private readonly ICacheService _cache;

    public GetCategoriesHandler(
        ICategoryRepository categoryRepository,
        IProductRepository productRepository,
        IMapper mapper,
        ICacheService cache)
    {
        _categoryRepository = categoryRepository;
        _productRepository = productRepository;
        _mapper = mapper;
        _cache = cache;
    }

    public async Task<IEnumerable<CategoryDto>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        var cached = await _cache.GetAsync<IEnumerable<CategoryDto>>(CacheKeys.AllCategories, cancellationToken);
        if (cached is not null)
            return cached;

        var categoriesTask = _categoryRepository.GetAllAsync(cancellationToken);
        var countByCategoryTask = _productRepository.GetCountByCategoryAsync(cancellationToken);

        await Task.WhenAll(categoriesTask, countByCategoryTask);

        var countMap = (await countByCategoryTask).ToDictionary(g => g.CategoryId, g => g.Count);
        var dtos = _mapper.Map<IEnumerable<CategoryDto>>(await categoriesTask).ToList();
        foreach (var dto in dtos)
            dto.ProductCount = countMap.GetValueOrDefault(dto.Id, 0);

        await _cache.SetAsync(CacheKeys.AllCategories, dtos, CacheDuration, cancellationToken);

        return dtos;
    }
}
