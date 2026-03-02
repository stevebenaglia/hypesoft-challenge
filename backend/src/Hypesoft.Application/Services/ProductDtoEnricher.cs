using Hypesoft.Application.DTOs;
using Hypesoft.Application.Interfaces;
using Hypesoft.Domain.Repositories;

namespace Hypesoft.Application.Services;

public sealed class ProductDtoEnricher : IProductDtoEnricher
{
    private readonly ICategoryRepository _categoryRepository;

    public ProductDtoEnricher(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task EnrichAsync(ProductDto dto, string categoryId, CancellationToken cancellationToken = default)
    {
        var category = await _categoryRepository.GetByIdAsync(categoryId, cancellationToken);
        dto.CategoryName = category?.Name;
    }

    public async Task EnrichManyAsync(IEnumerable<ProductDto> dtos, CancellationToken cancellationToken = default)
    {
        var dtoList = dtos as IList<ProductDto> ?? dtos.ToList();
        var categoryIds = dtoList.Select(d => d.CategoryId).Distinct().ToList();
        var categories = await _categoryRepository.GetByIdsAsync(categoryIds, cancellationToken);

        var categoryMap = categories.ToDictionary(c => c.Id, c => c.Name);

        foreach (var dto in dtoList)
            dto.CategoryName = categoryMap.GetValueOrDefault(dto.CategoryId);
    }
}
