using Hypesoft.Application.DTOs;
using MediatR;

namespace Hypesoft.Application.Commands.Products;

public sealed record UpdateProductCommand(
    string Id,
    string Name,
    string? Description,
    decimal Price,
    int StockQuantity,
    string CategoryId
) : IRequest<ProductDto>;
