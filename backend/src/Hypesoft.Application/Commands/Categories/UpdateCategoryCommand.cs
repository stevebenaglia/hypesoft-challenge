using Hypesoft.Application.DTOs;
using MediatR;

namespace Hypesoft.Application.Commands.Categories;

public sealed record UpdateCategoryCommand(
    string Id,
    string Name,
    string? Description
) : IRequest<CategoryDto>;
