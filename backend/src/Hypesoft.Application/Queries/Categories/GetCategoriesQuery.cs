using Hypesoft.Application.DTOs;
using MediatR;

namespace Hypesoft.Application.Queries.Categories;

/// <summary>Returns all categories as a simple list (no pagination).</summary>
public sealed record GetCategoriesQuery : IRequest<IEnumerable<CategoryDto>>;
