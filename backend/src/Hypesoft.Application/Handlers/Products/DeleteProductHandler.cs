using Hypesoft.Application.Commands.Products;
using Hypesoft.Application.DomainEvents;
using Hypesoft.Application.Interfaces;
using Hypesoft.Domain.DomainEvents.Products;
using Hypesoft.Domain.Exceptions;
using Hypesoft.Domain.Repositories;
using MediatR;

namespace Hypesoft.Application.Handlers.Products;

public sealed class DeleteProductHandler : IRequestHandler<DeleteProductCommand, Unit>
{
    private readonly IProductRepository _productRepository;
    private readonly IPublisher _publisher;
    private readonly ICacheService _cache;

    public DeleteProductHandler(IProductRepository productRepository, IPublisher publisher, ICacheService cache)
    {
        _productRepository = productRepository;
        _publisher = publisher;
        _cache = cache;
    }

    public async Task<Unit> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException("Product", request.Id);

        await _productRepository.DeleteAsync(product, cancellationToken);

        await Task.WhenAll(
            _cache.RemoveAsync(CacheKeys.ProductById(request.Id), cancellationToken),
            _cache.RemoveAsync(CacheKeys.DashboardSummary, cancellationToken));

        await _publisher.Publish(
            new DomainEventNotification<ProductDeletedEvent>(
                new ProductDeletedEvent(product.Id, product.Name)),
            cancellationToken);

        return Unit.Value;
    }
}
