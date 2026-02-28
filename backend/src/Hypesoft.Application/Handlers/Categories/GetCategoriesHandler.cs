using AutoMapper;
using Hypesoft.Application.DTOs;
using Hypesoft.Application.Interfaces;
using Hypesoft.Application.Queries.Categories;
using Hypesoft.Domain.Repositories;
using MediatR;

namespace Hypesoft.Application.Handlers.Categories;

public sealed class GetCategoriesHandler : IRequestHandler<GetCategoriesQuery, IEnumerable<CategoryDto>>
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

    private readonly ICategoryRepository _categoryRepository;
    private readonly IMapper _mapper;
    private readonly ICacheService _cache;

    public GetCategoriesHandler(ICategoryRepository categoryRepository, IMapper mapper, ICacheService cache)
    {
        _categoryRepository = categoryRepository;
        _mapper = mapper;
        _cache = cache;
    }

    public async Task<IEnumerable<CategoryDto>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        var cached = await _cache.GetAsync<IEnumerable<CategoryDto>>(CacheKeys.AllCategories, cancellationToken);
        if (cached is not null)
            return cached;

        var categories = await _categoryRepository.GetAllAsync(cancellationToken);
        var dtos = _mapper.Map<IEnumerable<CategoryDto>>(categories);

        await _cache.SetAsync(CacheKeys.AllCategories, dtos, CacheDuration, cancellationToken);

        return dtos;
    }
}
