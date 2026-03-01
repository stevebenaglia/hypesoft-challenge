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
    private readonly ICategoryRepository _categoryRepository;
    private readonly IMapper _mapper;
    private readonly IPublisher _publisher;
    private readonly ICacheService _cache;

    public UpdateProductHandler(
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

        await Task.WhenAll(
            _cache.RemoveAsync(CacheKeys.ProductById(request.Id), cancellationToken),
            _cache.RemoveAsync(CacheKeys.DashboardSummary, cancellationToken));

        await _publisher.Publish(
            new DomainEventNotification<ProductUpdatedEvent>(
                new ProductUpdatedEvent(product.Id, product.Name, product.Price, product.StockQuantity, product.CategoryId)),
            cancellationToken);

        var dto = _mapper.Map<ProductDto>(product);

        var category = await _categoryRepository.GetByIdAsync(effectiveCategoryId, cancellationToken);
        dto.CategoryName = category?.Name;

        return dto;
    }
}
