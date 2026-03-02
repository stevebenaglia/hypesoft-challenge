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

public sealed class UpdateStockHandlerTests
{
    private readonly Mock<IProductRepository> _productRepoMock = new();
    private readonly Mock<ICategoryRepository> _categoryRepoMock = new();
    private readonly Mock<IMapper> _mapperMock = new();
    private readonly Mock<IPublisher> _publisherMock = new();
    private readonly Mock<ICacheService> _cacheMock = new();
    private readonly UpdateStockHandler _handler;

    public UpdateStockHandlerTests()
    {
        _cacheMock
            .Setup(c => c.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _publisherMock
            .Setup(p => p.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _productRepoMock
            .Setup(r => r.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _handler = new UpdateStockHandler(
            _productRepoMock.Object,
            _categoryRepoMock.Object,
            _mapperMock.Object,
            _publisherMock.Object,
            _cacheMock.Object);
    }

    private static Product BuildProduct(string id = "prod-1", int stock = 20)
    {
        var name = ProductName.Create("Laptop");
        var price = Money.Create(999m);
        var qty = StockQuantity.Create(stock);
        return Product.Create(id, name, null, price, qty, "cat-1");
    }

    [Fact]
    public async Task Handle_ProductNotFound_ShouldThrowNotFoundException()
    {
        _productRepoMock
            .Setup(r => r.GetByIdAsync("nonexistent", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        var act = () => _handler.Handle(new UpdateStockCommand("nonexistent", 10), CancellationToken.None);

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
            .Returns(new ProductDto { Id = "prod-1", Name = "Laptop" });
        _categoryRepoMock
            .Setup(r => r.GetByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);

        await _handler.Handle(new UpdateStockCommand("prod-1", 50), CancellationToken.None);

        _productRepoMock.Verify(r => r.UpdateAsync(product, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ValidUpdate_ShouldInvalidateProductAndDashboardCaches()
    {
        var product = BuildProduct();
        _productRepoMock
            .Setup(r => r.GetByIdAsync("prod-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
        _mapperMock
            .Setup(m => m.Map<ProductDto>(It.IsAny<Product>()))
            .Returns(new ProductDto { Id = "prod-1", Name = "Laptop" });
        _categoryRepoMock
            .Setup(r => r.GetByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);

        await _handler.Handle(new UpdateStockCommand("prod-1", 50), CancellationToken.None);

        _cacheMock.Verify(
            c => c.RemoveAsync(CacheKeys.ProductById("prod-1"), It.IsAny<CancellationToken>()),
            Times.Once);
        _cacheMock.Verify(
            c => c.RemoveAsync(CacheKeys.DashboardSummary, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ValidUpdate_ShouldPublishStockUpdatedEvent()
    {
        var product = BuildProduct(stock: 20);
        _productRepoMock
            .Setup(r => r.GetByIdAsync("prod-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
        _mapperMock
            .Setup(m => m.Map<ProductDto>(It.IsAny<Product>()))
            .Returns(new ProductDto { Id = "prod-1", Name = "Laptop" });
        _categoryRepoMock
            .Setup(r => r.GetByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);

        await _handler.Handle(new UpdateStockCommand("prod-1", 50), CancellationToken.None);

        _publisherMock.Verify(
            p => p.Publish(
                It.Is<DomainEventNotification<StockUpdatedEvent>>(n =>
                    n.DomainEvent.ProductId == "prod-1" &&
                    n.DomainEvent.PreviousQuantity == 20 &&
                    n.DomainEvent.NewQuantity == 50),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ValidUpdate_ShouldReturnDtoWithCategoryName()
    {
        var product = BuildProduct();
        var dto = new ProductDto { Id = "prod-1", Name = "Laptop" };
        _productRepoMock
            .Setup(r => r.GetByIdAsync("prod-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
        _mapperMock.Setup(m => m.Map<ProductDto>(It.IsAny<Product>())).Returns(dto);
        var category = Category.Create("cat-1", "Electronics", null);
        _categoryRepoMock
            .Setup(r => r.GetByIdAsync("cat-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        var result = await _handler.Handle(new UpdateStockCommand("prod-1", 50), CancellationToken.None);

        result.CategoryName.Should().Be("Electronics");
    }
}
