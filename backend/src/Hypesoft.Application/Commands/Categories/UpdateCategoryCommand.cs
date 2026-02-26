using System.Text.Json.Serialization;
using Hypesoft.Application.DTOs;
using MediatR;

namespace Hypesoft.Application.Commands.Categories;

public sealed record UpdateCategoryCommand(
    [property: JsonIgnore] string Id,
    string Name,
    string? Description
) : IRequest<CategoryDto>;
