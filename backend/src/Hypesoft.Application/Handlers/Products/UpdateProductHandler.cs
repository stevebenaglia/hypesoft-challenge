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

public sealed class UpdateProductHandler : IRequestHandler<UpdateProductCommand, ProductDto>
{
    private readonly IProductRepository _productRepository;
    private readonly IMapper _mapper;
    private readonly IPublisher _publisher;
    private readonly ICacheInvalidationService _cacheInvalidation;
    private readonly IProductDtoEnricher _enricher;

    public UpdateProductHandler(
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

    public async Task<ProductDto> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException("Product", request.Id);

        // Keep existing category when CategoryId is not provided in the request.
        var effectiveCategoryId = !string.IsNullOrEmpty(request.CategoryId)
            ? request.CategoryId
            : product.CategoryId;

        var name = ProductName.Create(request.Name);
        var price = Money.Create(request.Price);
        var stock = StockQuantity.Create(request.StockQuantity);

        product.Update(name, request.Description, price, stock, effectiveCategoryId);

        await _productRepository.UpdateAsync(product, cancellationToken);

        await _cacheInvalidation.InvalidateProductMutationAsync(request.Id, cancellationToken);

        await _publisher.Publish(
            new DomainEventNotification<ProductUpdatedEvent>(
                new ProductUpdatedEvent(product.Id, product.Name, product.Price, product.StockQuantity, product.CategoryId)),
            cancellationToken);

        var dto = _mapper.Map<ProductDto>(product);
        await _enricher.EnrichAsync(dto, effectiveCategoryId, cancellationToken);

        return dto;
    }
}
