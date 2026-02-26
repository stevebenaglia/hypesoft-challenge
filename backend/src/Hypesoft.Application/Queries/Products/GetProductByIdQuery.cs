using Hypesoft.Application.DTOs;
using MediatR;

namespace Hypesoft.Application.Queries.Products;

public sealed record GetProductByIdQuery(string Id) : IRequest<ProductDto>;
