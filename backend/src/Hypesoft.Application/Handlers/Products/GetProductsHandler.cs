using AutoMapper;
using Hypesoft.Application.DTOs;
using Hypesoft.Application.Interfaces;
using Hypesoft.Application.Queries.Products;
using Hypesoft.Domain.Repositories;
using MediatR;

namespace Hypesoft.Application.Handlers.Products;

/// <summary>
/// Handles <see cref="GetProductsQuery"/>. Returns a paginated, optionally filtered and searched
/// list of products. Results are cached using a generation-counter key so that any product mutation
/// automatically invalidates all affected pages without requiring pattern-based key deletion.
/// </summary>
public sealed class GetProductsHandler : IRequestHandler<GetProductsQuery, PagedResultDto<ProductDto>>
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(2);

    private readonly IProductRepository _productRepository;
    private readonly IMapper _mapper;
    private readonly ICacheService _cache;
    private readonly ICacheInvalidationService _cacheInvalidation;
    private readonly IProductDtoEnricher _enricher;

    public GetProductsHandler(
        IProductRepository productRepository,
        IMapper mapper,
        ICacheService cache,
        ICacheInvalidationService cacheInvalidation,
        IProductDtoEnricher enricher)
    {
        _productRepository = productRepository;
        _mapper = mapper;
        _cache = cache;
        _cacheInvalidation = cacheInvalidation;
        _enricher = enricher;
    }

    public async Task<PagedResultDto<ProductDto>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        // The generation is bumped on every product mutation, effectively invalidating
        // all paginated product-list entries without pattern-based key deletion.
        var generation = await _cacheInvalidation.GetProductListGenerationAsync(cancellationToken);
        var cacheKey = CacheKeys.ProductList(generation, request.PageNumber, request.PageSize, request.SearchTerm, request.CategoryId, request.LowStockOnly);

        var cached = await _cache.GetAsync<PagedResultDto<ProductDto>>(cacheKey, cancellationToken);
        if (cached is not null)
            return cached;

        var (items, totalCount) = await _productRepository.GetPagedAsync(
            request.PageNumber,
            request.PageSize,
            request.SearchTerm,
            request.CategoryId,
            request.LowStockOnly,
            cancellationToken);

        var dtos = _mapper.Map<IEnumerable<ProductDto>>(items).ToList();

        await _enricher.EnrichManyAsync(dtos, cancellationToken);

        var result = PagedResultDto<ProductDto>.Create(dtos, totalCount, request.PageNumber, request.PageSize);

        await _cache.SetAsync(cacheKey, result, CacheDuration, cancellationToken);

        return result;
    }
}
