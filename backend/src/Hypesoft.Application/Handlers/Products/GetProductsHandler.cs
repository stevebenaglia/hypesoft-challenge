using AutoMapper;
using Hypesoft.Application.DTOs;
using Hypesoft.Application.Interfaces;
using Hypesoft.Application.Queries.Products;
using Hypesoft.Domain.Repositories;
using MediatR;

namespace Hypesoft.Application.Handlers.Products;

public sealed class GetProductsHandler : IRequestHandler<GetProductsQuery, PagedResultDto<ProductDto>>
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(2);

    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IMapper _mapper;
    private readonly ICacheService _cache;

    public GetProductsHandler(
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

    public async Task<PagedResultDto<ProductDto>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = CacheKeys.ProductList(request.PageNumber, request.PageSize, request.SearchTerm, request.CategoryId);

        var cached = await _cache.GetAsync<PagedResultDto<ProductDto>>(cacheKey, cancellationToken);
        if (cached is not null)
            return cached;

        var (items, totalCount) = await _productRepository.GetPagedAsync(
            request.PageNumber,
            request.PageSize,
            request.SearchTerm,
            request.CategoryId,
            cancellationToken);

        var dtos = _mapper.Map<IEnumerable<ProductDto>>(items).ToList();

        // Enrich with category names using a single $in query instead of N individual lookups
        var categoryIds = dtos.Select(d => d.CategoryId).Distinct().ToList();
        var categories = await _categoryRepository.GetByIdsAsync(categoryIds, cancellationToken);

        var categoryMap = categories.ToDictionary(c => c.Id, c => c.Name);

        foreach (var dto in dtos)
            dto.CategoryName = categoryMap.GetValueOrDefault(dto.CategoryId);

        var result = PagedResultDto<ProductDto>.Create(dtos, totalCount, request.PageNumber, request.PageSize);

        await _cache.SetAsync(cacheKey, result, CacheDuration, cancellationToken);

        return result;
    }
}
