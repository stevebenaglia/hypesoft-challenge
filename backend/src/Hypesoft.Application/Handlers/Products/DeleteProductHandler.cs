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
    private readonly ICacheInvalidationService _cacheInvalidation;

    public DeleteProductHandler(IProductRepository productRepository, IPublisher publisher, ICacheInvalidationService cacheInvalidation)
    {
        _productRepository = productRepository;
        _publisher = publisher;
        _cacheInvalidation = cacheInvalidation;
    }

    public async Task<Unit> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException("Product", request.Id);

        await _productRepository.DeleteAsync(product, cancellationToken);

        await _cacheInvalidation.InvalidateProductMutationAsync(request.Id, cancellationToken);

        await _publisher.Publish(
            new DomainEventNotification<ProductDeletedEvent>(
                new ProductDeletedEvent(product.Id, product.Name)),
            cancellationToken);

        return Unit.Value;
    }
}
