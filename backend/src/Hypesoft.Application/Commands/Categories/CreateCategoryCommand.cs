using Hypesoft.Application.DTOs;
using MediatR;

namespace Hypesoft.Application.Commands.Categories;

public sealed record CreateCategoryCommand(
    string Name,
    string? Description
) : IRequest<CategoryDto>;
