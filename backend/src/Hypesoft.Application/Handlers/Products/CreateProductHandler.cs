using AutoMapper;
using Hypesoft.Application.Commands.Products;
using Hypesoft.Application.DomainEvents;
using Hypesoft.Application.DTOs;
using Hypesoft.Application.Interfaces;
using Hypesoft.Domain.DomainEvents.Products;
using Hypesoft.Domain.Entities;
using Hypesoft.Domain.Repositories;
using Hypesoft.Domain.Services;
using Hypesoft.Domain.ValueObjects;
using MediatR;

namespace Hypesoft.Application.Handlers.Products;

public sealed class CreateProductHandler : IRequestHandler<CreateProductCommand, ProductDto>
{
    private readonly IProductRepository _productRepository;
    private readonly IIdGenerator _idGenerator;
    private readonly IMapper _mapper;
    private readonly IPublisher _publisher;
    private readonly ICacheInvalidationService _cacheInvalidation;
    private readonly IProductDtoEnricher _enricher;

    public CreateProductHandler(
        IProductRepository productRepository,
        IIdGenerator idGenerator,
        IMapper mapper,
        IPublisher publisher,
        ICacheInvalidationService cacheInvalidation,
        IProductDtoEnricher enricher)
    {
        _productRepository = productRepository;
        _idGenerator = idGenerator;
        _mapper = mapper;
        _publisher = publisher;
        _cacheInvalidation = cacheInvalidation;
        _enricher = enricher;
    }

    public async Task<ProductDto> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var name = ProductName.Create(request.Name);
        var price = Money.Create(request.Price);
        var stock = StockQuantity.Create(request.StockQuantity);

        var product = Product.Create(
            _idGenerator.NewId(),
            name,
            request.Description,
            price,
            stock,
            request.CategoryId);

        await _productRepository.AddAsync(product, cancellationToken);

        await _cacheInvalidation.InvalidateProductMutationAsync(productId: null, cancellationToken);

        await _publisher.Publish(
            new DomainEventNotification<ProductCreatedEvent>(
                new ProductCreatedEvent(product.Id, product.Name, product.Price, product.StockQuantity, product.CategoryId)),
            cancellationToken);

        var dto = _mapper.Map<ProductDto>(product);
        await _enricher.EnrichAsync(dto, request.CategoryId, cancellationToken);

        return dto;
    }
}
