using Hypesoft.Application.DTOs;
using MediatR;

namespace Hypesoft.Application.Queries.Categories;

public sealed record GetCategoryByIdQuery(string Id) : IRequest<CategoryDto>;
