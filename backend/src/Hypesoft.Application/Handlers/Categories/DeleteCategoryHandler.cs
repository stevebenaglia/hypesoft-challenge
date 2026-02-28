using Hypesoft.Application.Commands.Categories;
using Hypesoft.Application.DomainEvents;
using Hypesoft.Application.Interfaces;
using Hypesoft.Domain.DomainEvents.Categories;
using Hypesoft.Domain.Exceptions;
using Hypesoft.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Caching.Memory;

namespace Hypesoft.Application.Handlers.Categories;

public sealed class DeleteCategoryHandler : IRequestHandler<DeleteCategoryCommand, Unit>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IProductRepository _productRepository;
    private readonly IPublisher _publisher;
    private readonly IMemoryCache _cache;

    public DeleteCategoryHandler(ICategoryRepository categoryRepository, IProductRepository productRepository, IPublisher publisher, IMemoryCache cache)
    {
        _categoryRepository = categoryRepository;
        _productRepository = productRepository;
        _publisher = publisher;
        _cache = cache;
    }

    public async Task<Unit> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _categoryRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException("Category", request.Id);

        var hasProducts = await _productRepository.HasProductsInCategoryAsync(request.Id, cancellationToken);
        if (hasProducts)
            throw new DomainException($"Category '{category.Name}' cannot be deleted because it has associated products.");

        await _categoryRepository.DeleteAsync(category, cancellationToken);

        _cache.Remove(CacheKeys.AllCategories);

        await _publisher.Publish(
            new DomainEventNotification<CategoryDeletedEvent>(
                new CategoryDeletedEvent(category.Id, category.Name)),
            cancellationToken);

        return Unit.Value;
    }
}
