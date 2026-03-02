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
    private readonly Mock<IIdGenerator> _idGeneratorMock = new();
    private readonly Mock<IMapper> _mapperMock = new();
    private readonly Mock<IPublisher> _publisherMock = new();
    private readonly Mock<ICacheInvalidationService> _cacheInvalidationMock = new();
    private readonly Mock<IProductDtoEnricher> _enricherMock = new();
    private readonly CreateProductHandler _handler;

    public CreateProductHandlerTests()
    {
        _idGeneratorMock.Setup(g => g.NewId()).Returns("prod-1");
        _productRepoMock
            .Setup(r => r.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .Returns((Product p, CancellationToken _) => Task.FromResult(p));
        _cacheInvalidationMock
            .Setup(s => s.InvalidateProductMutationAsync(It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _publisherMock
            .Setup(p => p.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _enricherMock
            .Setup(e => e.EnrichAsync(It.IsAny<ProductDto>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _handler = new CreateProductHandler(
            _productRepoMock.Object,
            _idGeneratorMock.Object,
            _mapperMock.Object,
            _publisherMock.Object,
            _cacheInvalidationMock.Object,
            _enricherMock.Object);
    }

    private static CreateProductCommand ValidCommand() =>
        new("Laptop", null, 2999.99m, 20, "cat-1");

    [Fact]
    public async Task Handle_ValidCommand_ShouldCallAddAsync()
    {
        _mapperMock
            .Setup(m => m.Map<ProductDto>(It.IsAny<Product>()))
            .Returns(new ProductDto { Id = "prod-1", Name = "Laptop" });

        await _handler.Handle(ValidCommand(), CancellationToken.None);

        _productRepoMock.Verify(
            r => r.AddAsync(It.Is<Product>(p => p.Name == "Laptop"), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldInvalidateProductMutation()
    {
        _mapperMock
            .Setup(m => m.Map<ProductDto>(It.IsAny<Product>()))
            .Returns(new ProductDto { Id = "prod-1", Name = "Laptop" });

        await _handler.Handle(ValidCommand(), CancellationToken.None);

        _cacheInvalidationMock.Verify(
            s => s.InvalidateProductMutationAsync(null, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldPublishProductCreatedEvent()
    {
        _mapperMock
            .Setup(m => m.Map<ProductDto>(It.IsAny<Product>()))
            .Returns(new ProductDto { Id = "prod-1", Name = "Laptop" });

        await _handler.Handle(ValidCommand(), CancellationToken.None);

        _publisherMock.Verify(
            p => p.Publish(
                It.Is<DomainEventNotification<ProductCreatedEvent>>(n => n.DomainEvent.Name == "Laptop"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldCallEnrichAsync()
    {
        _mapperMock
            .Setup(m => m.Map<ProductDto>(It.IsAny<Product>()))
            .Returns(new ProductDto { Id = "prod-1", Name = "Laptop" });

        await _handler.Handle(ValidCommand(), CancellationToken.None);

        _enricherMock.Verify(
            e => e.EnrichAsync(It.IsAny<ProductDto>(), "cat-1", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldReturnDtoWithCategoryName()
    {
        var dto = new ProductDto { Id = "prod-1", Name = "Laptop" };
        _mapperMock.Setup(m => m.Map<ProductDto>(It.IsAny<Product>())).Returns(dto);
        _enricherMock
            .Setup(e => e.EnrichAsync(It.IsAny<ProductDto>(), "cat-1", It.IsAny<CancellationToken>()))
            .Callback((ProductDto d, string _, CancellationToken _) => d.CategoryName = "Electronics")
            .Returns(Task.CompletedTask);

        var result = await _handler.Handle(ValidCommand(), CancellationToken.None);

        result.CategoryName.Should().Be("Electronics");
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldReturnMappedDto()
    {
        var expected = new ProductDto { Id = "prod-1", Name = "Laptop" };
        _mapperMock.Setup(m => m.Map<ProductDto>(It.IsAny<Product>())).Returns(expected);

        var result = await _handler.Handle(ValidCommand(), CancellationToken.None);

        result.Should().Be(expected);
    }
}
