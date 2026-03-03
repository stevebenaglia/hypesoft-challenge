using Hypesoft.Application.DTOs;
using MediatR;

namespace Hypesoft.Application.Queries.Products;

public sealed record GetProductsQuery(
    int PageNumber = 1,
    int PageSize = 10,
    string? SearchTerm = null,
    string? CategoryId = null,
    bool LowStockOnly = false
) : IRequest<PagedResultDto<ProductDto>>;
