using FluentAssertions;
using Hypesoft.Application.Commands.Products;
using Hypesoft.Application.DomainEvents;
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

public sealed class DeleteProductHandlerTests
{
    private readonly Mock<IProductRepository> _productRepoMock = new();
    private readonly Mock<IPublisher> _publisherMock = new();
    private readonly Mock<ICacheInvalidationService> _cacheInvalidationMock = new();
    private readonly DeleteProductHandler _handler;

    public DeleteProductHandlerTests()
    {
        _cacheInvalidationMock
            .Setup(s => s.InvalidateProductMutationAsync(It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _publisherMock
            .Setup(p => p.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _productRepoMock
            .Setup(r => r.DeleteAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _handler = new DeleteProductHandler(
            _productRepoMock.Object,
            _publisherMock.Object,
            _cacheInvalidationMock.Object);
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

        var act = () => _handler.Handle(new DeleteProductCommand("nonexistent"), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*Product*nonexistent*");
    }

    [Fact]
    public async Task Handle_ValidDelete_ShouldCallDeleteAsync()
    {
        var product = BuildProduct();
        _productRepoMock
            .Setup(r => r.GetByIdAsync("prod-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        await _handler.Handle(new DeleteProductCommand("prod-1"), CancellationToken.None);

        _productRepoMock.Verify(r => r.DeleteAsync(product, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ValidDelete_ShouldInvalidateProductMutation()
    {
        var product = BuildProduct();
        _productRepoMock
            .Setup(r => r.GetByIdAsync("prod-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        await _handler.Handle(new DeleteProductCommand("prod-1"), CancellationToken.None);

        _cacheInvalidationMock.Verify(
            s => s.InvalidateProductMutationAsync("prod-1", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ValidDelete_ShouldPublishProductDeletedEvent()
    {
        var product = BuildProduct();
        _productRepoMock
            .Setup(r => r.GetByIdAsync("prod-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        await _handler.Handle(new DeleteProductCommand("prod-1"), CancellationToken.None);

        _publisherMock.Verify(
            p => p.Publish(
                It.Is<DomainEventNotification<ProductDeletedEvent>>(n =>
                    n.DomainEvent.ProductId == "prod-1" && n.DomainEvent.Name == "Laptop"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
