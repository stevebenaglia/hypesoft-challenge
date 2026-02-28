using AutoMapper;
using Hypesoft.Application.Commands.Categories;
using Hypesoft.Application.DTOs;
using Hypesoft.Application.Interfaces;
using Hypesoft.Domain.Exceptions;
using Hypesoft.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Caching.Memory;

namespace Hypesoft.Application.Handlers.Categories;

public sealed class UpdateCategoryHandler : IRequestHandler<UpdateCategoryCommand, CategoryDto>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IMapper _mapper;
    private readonly IMemoryCache _cache;

    public UpdateCategoryHandler(ICategoryRepository categoryRepository, IMapper mapper, IMemoryCache cache)
    {
        _categoryRepository = categoryRepository;
        _mapper = mapper;
        _cache = cache;
    }

    public async Task<CategoryDto> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _categoryRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException("Category", request.Id);

        category.Update(request.Name, request.Description);

        await _categoryRepository.UpdateAsync(category, cancellationToken);

        _cache.Remove(CacheKeys.AllCategories);

        return _mapper.Map<CategoryDto>(category);
    }
}
