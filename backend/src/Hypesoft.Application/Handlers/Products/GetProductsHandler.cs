using AutoMapper;
using Hypesoft.Application.DTOs;
using Hypesoft.Application.Queries.Products;
using Hypesoft.Domain.Repositories;
using MediatR;

namespace Hypesoft.Application.Handlers.Products;

public sealed class GetProductsHandler : IRequestHandler<GetProductsQuery, PagedResultDto<ProductDto>>
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IMapper _mapper;

    public GetProductsHandler(
        IProductRepository productRepository,
        ICategoryRepository categoryRepository,
        IMapper mapper)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _mapper = mapper;
    }

    public async Task<PagedResultDto<ProductDto>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
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

        return PagedResultDto<ProductDto>.Create(dtos, totalCount, request.PageNumber, request.PageSize);
    }
}
