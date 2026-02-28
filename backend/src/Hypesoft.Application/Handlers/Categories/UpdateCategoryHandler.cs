using AutoMapper;
using Hypesoft.Application.Commands.Categories;
using Hypesoft.Application.DomainEvents;
using Hypesoft.Application.DTOs;
using Hypesoft.Application.Interfaces;
using Hypesoft.Domain.DomainEvents.Categories;
using Hypesoft.Domain.Exceptions;
using Hypesoft.Domain.Repositories;
using MediatR;

namespace Hypesoft.Application.Handlers.Categories;

public sealed class UpdateCategoryHandler : IRequestHandler<UpdateCategoryCommand, CategoryDto>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IMapper _mapper;
    private readonly IPublisher _publisher;
    private readonly ICacheService _cache;

    public UpdateCategoryHandler(
        ICategoryRepository categoryRepository,
        IMapper mapper,
        IPublisher publisher,
        ICacheService cache)
    {
        _categoryRepository = categoryRepository;
        _mapper = mapper;
        _publisher = publisher;
        _cache = cache;
    }

    public async Task<CategoryDto> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _categoryRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException("Category", request.Id);

        category.Update(request.Name, request.Description);

        await _categoryRepository.UpdateAsync(category, cancellationToken);

        await _cache.RemoveAsync(CacheKeys.AllCategories, cancellationToken);

        await _publisher.Publish(
            new DomainEventNotification<CategoryUpdatedEvent>(
                new CategoryUpdatedEvent(category.Id, category.Name, category.Description)),
            cancellationToken);

        return _mapper.Map<CategoryDto>(category);
    }
}
