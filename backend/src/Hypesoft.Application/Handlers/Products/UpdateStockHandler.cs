using AutoMapper;
using Hypesoft.Application.Commands.Products;
using Hypesoft.Application.DomainEvents;
using Hypesoft.Application.DTOs;
using Hypesoft.Application.Interfaces;
using Hypesoft.Domain.DomainEvents.Products;
using Hypesoft.Domain.Exceptions;
using Hypesoft.Domain.Repositories;
using Hypesoft.Domain.ValueObjects;
using MediatR;

namespace Hypesoft.Application.Handlers.Products;

/// <summary>
/// Handles <see cref="UpdateStockCommand"/>. Loads the product, applies the new
/// <see cref="Hypesoft.Domain.ValueObjects.StockQuantity"/> via the domain method,
/// persists the change, invalidates the product's individual cache entry plus the
/// product-list generation and dashboard, then publishes a
/// <see cref="Hypesoft.Domain.DomainEvents.Products.StockUpdatedEvent"/>.
/// </summary>
public sealed class UpdateStockHandler : IRequestHandler<UpdateStockCommand, ProductDto>
{
    private readonly IProductRepository _productRepository;
    private readonly IMapper _mapper;
    private readonly IPublisher _publisher;
    private readonly ICacheInvalidationService _cacheInvalidation;
    private readonly IProductDtoEnricher _enricher;

    public UpdateStockHandler(
        IProductRepository productRepository,
        IMapper mapper,
        IPublisher publisher,
        ICacheInvalidationService cacheInvalidation,
        IProductDtoEnricher enricher)
    {
        _productRepository = productRepository;
        _mapper = mapper;
        _publisher = publisher;
        _cacheInvalidation = cacheInvalidation;
        _enricher = enricher;
    }

    public async Task<ProductDto> Handle(UpdateStockCommand request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException("Product", request.Id);

        var previousQuantity = product.StockQuantity;
        var newStock = StockQuantity.Create(request.Quantity);

        product.UpdateStock(newStock);

        await _productRepository.UpdateAsync(product, cancellationToken);

        await _cacheInvalidation.InvalidateProductMutationAsync(request.Id, cancellationToken);

        await _publisher.Publish(
            new DomainEventNotification<StockUpdatedEvent>(
                new StockUpdatedEvent(product.Id, previousQuantity, product.StockQuantity)),
            cancellationToken);

        var dto = _mapper.Map<ProductDto>(product);
        await _enricher.EnrichAsync(dto, product.CategoryId, cancellationToken);

        return dto;
    }
}
