using Hypesoft.Application.DTOs;

namespace Hypesoft.Application.Interfaces;

public interface IProductDtoEnricher
{
    /// <summary>Fetches the category name and sets <see cref="ProductDto.CategoryName"/> on the DTO.</summary>
    Task EnrichAsync(ProductDto dto, string categoryId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Enriches multiple DTOs in a single category query ($in operator).
    /// Each DTO's <see cref="ProductDto.CategoryId"/> is used to look up the category name.
    /// </summary>
    Task EnrichManyAsync(IEnumerable<ProductDto> dtos, CancellationToken cancellationToken = default);
}
