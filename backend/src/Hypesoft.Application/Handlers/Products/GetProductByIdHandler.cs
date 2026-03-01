using AutoMapper;
using Hypesoft.Application.DTOs;
using Hypesoft.Application.Interfaces;
using Hypesoft.Application.Queries.Products;
using Hypesoft.Domain.Exceptions;
using Hypesoft.Domain.Repositories;
using MediatR;

namespace Hypesoft.Application.Handlers.Products;

public sealed class GetProductByIdHandler : IRequestHandler<GetProductByIdQuery, ProductDto>
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IMapper _mapper;
    private readonly ICacheService _cache;

    public GetProductByIdHandler(
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

    public async Task<ProductDto> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = CacheKeys.ProductById(request.Id);

        var cached = await _cache.GetAsync<ProductDto>(cacheKey, cancellationToken);
        if (cached is not null)
            return cached;

        var product = await _productRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException("Product", request.Id);

        var dto = _mapper.Map<ProductDto>(product);

        var category = await _categoryRepository.GetByIdAsync(product.CategoryId, cancellationToken);
        dto.CategoryName = category?.Name;

        await _cache.SetAsync(cacheKey, dto, CacheDuration, cancellationToken);

        return dto;
    }
}
