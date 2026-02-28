using AutoMapper;
using Hypesoft.Application.Commands.Products;
using Hypesoft.Application.DomainEvents;
using Hypesoft.Application.DTOs;
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
    private readonly ICategoryRepository _categoryRepository;
    private readonly IIdGenerator _idGenerator;
    private readonly IMapper _mapper;
    private readonly IPublisher _publisher;

    public CreateProductHandler(
        IProductRepository productRepository,
        ICategoryRepository categoryRepository,
        IIdGenerator idGenerator,
        IMapper mapper,
        IPublisher publisher)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _idGenerator = idGenerator;
        _mapper = mapper;
        _publisher = publisher;
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

        await _publisher.Publish(
            new DomainEventNotification<ProductCreatedEvent>(
                new ProductCreatedEvent(product.Id, product.Name, product.Price, product.StockQuantity, product.CategoryId)),
            cancellationToken);

        var dto = _mapper.Map<ProductDto>(product);

        var category = await _categoryRepository.GetByIdAsync(request.CategoryId, cancellationToken);
        dto.CategoryName = category?.Name;

        return dto;
    }
}
