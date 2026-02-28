using AutoMapper;
using Hypesoft.Application.DTOs;
using Hypesoft.Application.Interfaces;
using Hypesoft.Application.Queries.Categories;
using Hypesoft.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Caching.Memory;

namespace Hypesoft.Application.Handlers.Categories;

public sealed class GetCategoriesHandler : IRequestHandler<GetCategoriesQuery, IEnumerable<CategoryDto>>
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

    private readonly ICategoryRepository _categoryRepository;
    private readonly IMapper _mapper;
    private readonly IMemoryCache _cache;

    public GetCategoriesHandler(ICategoryRepository categoryRepository, IMapper mapper, IMemoryCache cache)
    {
        _categoryRepository = categoryRepository;
        _mapper = mapper;
        _cache = cache;
    }

    public async Task<IEnumerable<CategoryDto>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        if (_cache.TryGetValue(CacheKeys.AllCategories, out IEnumerable<CategoryDto>? cached))
            return cached!;

        var categories = await _categoryRepository.GetAllAsync(cancellationToken);
        var dtos = _mapper.Map<IEnumerable<CategoryDto>>(categories);

        _cache.Set(CacheKeys.AllCategories, dtos, CacheDuration);

        return dtos;
    }
}
