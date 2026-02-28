using System.Text.Json.Serialization;
using Hypesoft.Application.DTOs;
using MediatR;

namespace Hypesoft.Application.Commands.Products;

public sealed record UpdateStockCommand(
    [property: JsonIgnore] string Id,
    int Quantity
) : IRequest<ProductDto>;
