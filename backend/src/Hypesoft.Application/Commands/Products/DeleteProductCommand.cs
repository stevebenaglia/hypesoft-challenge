using MediatR;

namespace Hypesoft.Application.Commands.Products;

public sealed record DeleteProductCommand(string Id) : IRequest<Unit>;
