using AutoMapper;
using FluentAssertions;
using Hypesoft.Application.Commands.Products;
using Hypesoft.Application.DomainEvents;
using Hypesoft.Application.DTOs;
using Hypesoft.Application.Handlers.Products;
using Hypesoft.Application.Interfaces;
using Hypesoft.Domain.DomainEvents.Products;
using Hypesoft.Domain.Entities;
using Hypesoft.Domain.Exceptions;
using Hypesoft.Domain.Repositories;
using Hypesoft.Domain.ValueObjects;
using MediatR;
using Moq;

namespace Hypesoft.UnitTests.Handlers.Products;

public sealed class UpdateProductHandlerTests
{
    private readonly Mock<IProductRepository> _productRepoMock = new();
    private readonly Mock<IMapper> _mapperMock = new();
    private readonly Mock<IPublisher> _publisherMock = new();
    private readonly Mock<ICacheInvalidationService> _cacheInvalidationMock = new();
    private readonly Mock<IProductDtoEnricher> _enricherMock = new();
    private readonly UpdateProductHandler _handler;

    public UpdateProductHandlerTests()
    {
        _cacheInvalidationMock
            .Setup(s => s.InvalidateProductMutationAsync(It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _publisherMock
            .Setup(p => p.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _productRepoMock
            .Setup(r => r.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _enricherMock
            .Setup(e => e.EnrichAsync(It.IsAny<ProductDto>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _handler = new UpdateProductHandler(
            _productRepoMock.Object,
            _mapperMock.Object,
            _publisherMock.Object,
            _cacheInvalidationMock.Object,
            _enricherMock.Object);
    }

    private static Product BuildProduct(string id = "prod-1")
    {
        var name = ProductName.Create("Laptop");
        var price = Money.Create(999m);
        var qty = StockQuantity.Create(20);
        return Product.Create(id, name, null, price, qty, "cat-1");
    }

    [Fact]
    public async Task Handle_ProductNotFound_ShouldThrowNotFoundException()
    {
        _productRepoMock
            .Setup(r => r.GetByIdAsync("nonexistent", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        var command = new UpdateProductCommand("nonexistent", "Laptop", null, 999m, 20, null);
        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*Product*nonexistent*");
    }

    [Fact]
    public async Task Handle_ValidUpdate_ShouldCallUpdateAsync()
    {
        var product = BuildProduct();
        _productRepoMock
            .Setup(r => r.GetByIdAsync("prod-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
        _mapperMock
            .Setup(m => m.Map<ProductDto>(It.IsAny<Product>()))
            .Returns(new ProductDto { Id = "prod-1", Name = "Updated Laptop" });

        var command = new UpdateProductCommand("prod-1", "Updated Laptop", null, 1099m, 15, "cat-1");
        await _handler.Handle(command, CancellationToken.None);

        _productRepoMock.Verify(r => r.UpdateAsync(product, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ValidUpdate_ShouldInvalidateProductMutation()
    {
        var product = BuildProduct();
        _productRepoMock
            .Setup(r => r.GetByIdAsync("prod-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
        _mapperMock
            .Setup(m => m.Map<ProductDto>(It.IsAny<Product>()))
            .Returns(new ProductDto { Id = "prod-1", Name = "Updated Laptop" });

        var command = new UpdateProductCommand("prod-1", "Updated Laptop", null, 1099m, 15, "cat-1");
        await _handler.Handle(command, CancellationToken.None);

        _cacheInvalidationMock.Verify(
            s => s.InvalidateProductMutationAsync("prod-1", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ValidUpdate_ShouldPublishProductUpdatedEvent()
    {
        var product = BuildProduct();
        _productRepoMock
            .Setup(r => r.GetByIdAsync("prod-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
        _mapperMock
            .Setup(m => m.Map<ProductDto>(It.IsAny<Product>()))
            .Returns(new ProductDto { Id = "prod-1", Name = "Updated Laptop" });

        var command = new UpdateProductCommand("prod-1", "Updated Laptop", null, 1099m, 15, "cat-1");
        await _handler.Handle(command, CancellationToken.None);

        _publisherMock.Verify(
            p => p.Publish(
                It.Is<DomainEventNotification<ProductUpdatedEvent>>(n => n.DomainEvent.ProductId == "prod-1"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_NoCategoryIdInCommand_ShouldKeepExistingCategory()
    {
        var product = BuildProduct();
        _productRepoMock
            .Setup(r => r.GetByIdAsync("prod-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
        _mapperMock
            .Setup(m => m.Map<ProductDto>(It.IsAny<Product>()))
            .Returns(new ProductDto { Id = "prod-1", Name = "Laptop", CategoryId = "cat-1" });

        // No CategoryId provided — should fall back to product.CategoryId = "cat-1"
        var command = new UpdateProductCommand("prod-1", "Laptop", null, 999m, 20, null);
        await _handler.Handle(command, CancellationToken.None);

        _enricherMock.Verify(
            e => e.EnrichAsync(It.IsAny<ProductDto>(), "cat-1", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ValidUpdate_ShouldReturnDtoWithCategoryName()
    {
        var product = BuildProduct();
        var dto = new ProductDto { Id = "prod-1", Name = "Updated Laptop" };
        _productRepoMock
            .Setup(r => r.GetByIdAsync("prod-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
        _mapperMock.Setup(m => m.Map<ProductDto>(It.IsAny<Product>())).Returns(dto);
        _enricherMock
            .Setup(e => e.EnrichAsync(It.IsAny<ProductDto>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Callback((ProductDto d, string _, CancellationToken _) => d.CategoryName = "Electronics")
            .Returns(Task.CompletedTask);

        var command = new UpdateProductCommand("prod-1", "Updated Laptop", null, 1099m, 15, "cat-1");
        var result = await _handler.Handle(command, CancellationToken.None);

        result.CategoryName.Should().Be("Electronics");
    }
}
