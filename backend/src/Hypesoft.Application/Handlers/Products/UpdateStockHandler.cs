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

public sealed class UpdateStockHandler : IRequestHandler<UpdateStockCommand, ProductDto>
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IMapper _mapper;
    private readonly IPublisher _publisher;
    private readonly ICacheService _cache;

    public UpdateStockHandler(
        IProductRepository productRepository,
        ICategoryRepository categoryRepository,
        IMapper mapper,
        IPublisher publisher,
        ICacheService cache)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _mapper = mapper;
        _publisher = publisher;
        _cache = cache;
    }

    public async Task<ProductDto> Handle(UpdateStockCommand request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException("Product", request.Id);

        var previousQuantity = product.StockQuantity;
        var newStock = StockQuantity.Create(request.Quantity);

        product.UpdateStock(newStock);

        await _productRepository.UpdateAsync(product, cancellationToken);

        await Task.WhenAll(
            _cache.RemoveAsync(CacheKeys.ProductById(request.Id), cancellationToken),
            _cache.RemoveAsync(CacheKeys.DashboardSummary, cancellationToken));

        await _publisher.Publish(
            new DomainEventNotification<StockUpdatedEvent>(
                new StockUpdatedEvent(product.Id, previousQuantity, product.StockQuantity)),
            cancellationToken);

        var dto = _mapper.Map<ProductDto>(product);

        var category = await _categoryRepository.GetByIdAsync(product.CategoryId, cancellationToken);
        dto.CategoryName = category?.Name;

        return dto;
    }
}
