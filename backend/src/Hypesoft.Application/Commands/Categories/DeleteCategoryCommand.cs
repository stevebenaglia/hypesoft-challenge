using MediatR;

namespace Hypesoft.Application.Commands.Categories;

public sealed record DeleteCategoryCommand(string Id) : IRequest<Unit>;
