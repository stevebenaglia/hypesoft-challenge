using AutoMapper;
using Hypesoft.Application.Commands.Categories;
using Hypesoft.Application.DomainEvents;
using Hypesoft.Application.DTOs;
using Hypesoft.Application.Interfaces;
using Hypesoft.Domain.DomainEvents.Categories;
using Hypesoft.Domain.Entities;
using Hypesoft.Domain.Repositories;
using Hypesoft.Domain.Services;
using MediatR;

namespace Hypesoft.Application.Handlers.Categories;

public sealed class CreateCategoryHandler : IRequestHandler<CreateCategoryCommand, CategoryDto>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IIdGenerator _idGenerator;
    private readonly IMapper _mapper;
    private readonly IPublisher _publisher;
    private readonly ICacheService _cache;

    public CreateCategoryHandler(
        ICategoryRepository categoryRepository,
        IIdGenerator idGenerator,
        IMapper mapper,
        IPublisher publisher,
        ICacheService cache)
    {
        _categoryRepository = categoryRepository;
        _idGenerator = idGenerator;
        _mapper = mapper;
        _publisher = publisher;
        _cache = cache;
    }

    public async Task<CategoryDto> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = Category.Create(_idGenerator.NewId(), request.Name, request.Description);

        await _categoryRepository.AddAsync(category, cancellationToken);

        await _cache.RemoveAsync(CacheKeys.AllCategories, cancellationToken);

        await _publisher.Publish(
            new DomainEventNotification<CategoryCreatedEvent>(
                new CategoryCreatedEvent(category.Id, category.Name)),
            cancellationToken);

        return _mapper.Map<CategoryDto>(category);
    }
}
