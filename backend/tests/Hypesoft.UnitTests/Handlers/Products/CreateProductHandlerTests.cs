using AutoMapper;
using FluentAssertions;
using Hypesoft.Application.Commands.Products;
using Hypesoft.Application.DomainEvents;
using Hypesoft.Application.DTOs;
using Hypesoft.Application.Handlers.Products;
using Hypesoft.Application.Interfaces;
using Hypesoft.Domain.DomainEvents.Products;
using Hypesoft.Domain.Entities;
using Hypesoft.Domain.Repositories;
using Hypesoft.Domain.Services;
using MediatR;
using Moq;

namespace Hypesoft.UnitTests.Handlers.Products;

public sealed class CreateProductHandlerTests
{
    private readonly Mock<IProductRepository> _productRepoMock = new();
    private readonly Mock<ICategoryRepository> _categoryRepoMock = new();
    private readonly Mock<IIdGenerator> _idGeneratorMock = new();
    private readonly Mock<IMapper> _mapperMock = new();
    private readonly Mock<IPublisher> _publisherMock = new();
    private readonly Mock<ICacheService> _cacheMock = new();
    private readonly CreateProductHandler _handler;

    public CreateProductHandlerTests()
    {
        _idGeneratorMock.Setup(g => g.NewId()).Returns("prod-1");
        _productRepoMock
            .Setup(r => r.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .Returns((Product p, CancellationToken _) => Task.FromResult(p));
        _cacheMock
            .Setup(c => c.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _publisherMock
            .Setup(p => p.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _handler = new CreateProductHandler(
            _productRepoMock.Object,
            _categoryRepoMock.Object,
            _idGeneratorMock.Object,
            _mapperMock.Object,
            _publisherMock.Object,
            _cacheMock.Object);
    }

    private static CreateProductCommand ValidCommand() =>
        new("Laptop", null, 2999.99m, 20, "cat-1");

    [Fact]
    public async Task Handle_ValidCommand_ShouldCallAddAsync()
    {
        _mapperMock
            .Setup(m => m.Map<ProductDto>(It.IsAny<Product>()))
            .Returns(new ProductDto { Id = "prod-1", Name = "Laptop" });
        _categoryRepoMock
            .Setup(r => r.GetByIdAsync("cat-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);

        await _handler.Handle(ValidCommand(), CancellationToken.None);

        _productRepoMock.Verify(
            r => r.AddAsync(It.Is<Product>(p => p.Name == "Laptop"), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldInvalidateDashboardCache()
    {
        _mapperMock
            .Setup(m => m.Map<ProductDto>(It.IsAny<Product>()))
            .Returns(new ProductDto { Id = "prod-1", Name = "Laptop" });
        _categoryRepoMock
            .Setup(r => r.GetByIdAsync("cat-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);

        await _handler.Handle(ValidCommand(), CancellationToken.None);

        _cacheMock.Verify(
            c => c.RemoveAsync(CacheKeys.DashboardSummary, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldPublishProductCreatedEvent()
    {
        _mapperMock
            .Setup(m => m.Map<ProductDto>(It.IsAny<Product>()))
            .Returns(new ProductDto { Id = "prod-1", Name = "Laptop" });
        _categoryRepoMock
            .Setup(r => r.GetByIdAsync("cat-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);

        await _handler.Handle(ValidCommand(), CancellationToken.None);

        _publisherMock.Verify(
            p => p.Publish(
                It.Is<DomainEventNotification<ProductCreatedEvent>>(n => n.DomainEvent.Name == "Laptop"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldReturnDtoWithCategoryName()
    {
        var dto = new ProductDto { Id = "prod-1", Name = "Laptop" };
        _mapperMock.Setup(m => m.Map<ProductDto>(It.IsAny<Product>())).Returns(dto);
        var category = Category.Create("cat-1", "Electronics", null);
        _categoryRepoMock
            .Setup(r => r.GetByIdAsync("cat-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        var result = await _handler.Handle(ValidCommand(), CancellationToken.None);

        result.CategoryName.Should().Be("Electronics");
    }

    [Fact]
    public async Task Handle_CategoryNotFound_ShouldReturnDtoWithNullCategoryName()
    {
        var dto = new ProductDto { Id = "prod-1", Name = "Laptop" };
        _mapperMock.Setup(m => m.Map<ProductDto>(It.IsAny<Product>())).Returns(dto);
        _categoryRepoMock
            .Setup(r => r.GetByIdAsync("cat-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);

        var result = await _handler.Handle(ValidCommand(), CancellationToken.None);

        result.CategoryName.Should().BeNull();
    }
}
