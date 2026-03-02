using AutoMapper;
using FluentAssertions;
using Hypesoft.Application.DTOs;
using Hypesoft.Application.Handlers.Products;
using Hypesoft.Application.Interfaces;
using Hypesoft.Application.Queries.Products;
using Hypesoft.Domain.Entities;
using Hypesoft.Domain.Repositories;
using Hypesoft.Domain.ValueObjects;
using Moq;

namespace Hypesoft.UnitTests.Handlers.Products;

public sealed class GetProductsHandlerTests
{
    private readonly Mock<IProductRepository> _productRepoMock = new();
    private readonly Mock<IMapper> _mapperMock = new();
    private readonly Mock<ICacheService> _cacheMock = new();
    private readonly Mock<ICacheInvalidationService> _cacheInvalidationMock = new();
    private readonly Mock<IProductDtoEnricher> _enricherMock = new();
    private readonly GetProductsHandler _handler;

    public GetProductsHandlerTests()
    {
        _cacheMock
            .Setup(c => c.GetAsync<PagedResultDto<ProductDto>>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PagedResultDto<ProductDto>?)null);
        _cacheMock
            .Setup(c => c.SetAsync(It.IsAny<string>(), It.IsAny<PagedResultDto<ProductDto>>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _cacheInvalidationMock
            .Setup(s => s.GetProductListGenerationAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);
        _enricherMock
            .Setup(e => e.EnrichManyAsync(It.IsAny<IEnumerable<ProductDto>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _handler = new GetProductsHandler(
            _productRepoMock.Object,
            _mapperMock.Object,
            _cacheMock.Object,
            _cacheInvalidationMock.Object,
            _enricherMock.Object);
    }

    private static Product BuildProduct(string id, string name, string categoryId)
    {
        var n = ProductName.Create(name);
        var price = Money.Create(99m);
        var qty = StockQuantity.Create(10);
        return Product.Create(id, n, null, price, qty, categoryId);
    }

    [Fact]
    public async Task Handle_CacheHit_ShouldReturnCachedResult()
    {
        var cached = PagedResultDto<ProductDto>.Create(
            new List<ProductDto> { new() { Id = "p1", Name = "Cached" } }, 1, 1, 10);

        _cacheMock
            .Setup(c => c.GetAsync<PagedResultDto<ProductDto>>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(cached);

        var result = await _handler.Handle(new GetProductsQuery(), CancellationToken.None);

        result.Should().Be(cached);
        _productRepoMock.Verify(r => r.GetPagedAsync(
            It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_CacheMiss_ShouldCallGetPagedAsync()
    {
        var products = new List<Product> { BuildProduct("p1", "Laptop", "cat-1") };
        _productRepoMock
            .Setup(r => r.GetPagedAsync(1, 10, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync((products, 1));
        _mapperMock
            .Setup(m => m.Map<IEnumerable<ProductDto>>(It.IsAny<IEnumerable<Product>>()))
            .Returns(new List<ProductDto> { new() { Id = "p1", Name = "Laptop", CategoryId = "cat-1" } });

        await _handler.Handle(new GetProductsQuery(), CancellationToken.None);

        _productRepoMock.Verify(r => r.GetPagedAsync(1, 10, null, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_CacheMiss_ShouldCallEnrichManyAsync()
    {
        var products = new List<Product> { BuildProduct("p1", "Laptop", "cat-1") };
        _productRepoMock
            .Setup(r => r.GetPagedAsync(1, 10, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync((products, 1));
        _mapperMock
            .Setup(m => m.Map<IEnumerable<ProductDto>>(It.IsAny<IEnumerable<Product>>()))
            .Returns(new List<ProductDto> { new() { Id = "p1", Name = "Laptop", CategoryId = "cat-1" } });

        await _handler.Handle(new GetProductsQuery(), CancellationToken.None);

        _enricherMock.Verify(
            e => e.EnrichManyAsync(It.IsAny<IEnumerable<ProductDto>>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_CacheMiss_ShouldStoreResultInCache()
    {
        var products = new List<Product> { BuildProduct("p1", "Laptop", "cat-1") };
        _productRepoMock
            .Setup(r => r.GetPagedAsync(1, 10, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync((products, 1));
        _mapperMock
            .Setup(m => m.Map<IEnumerable<ProductDto>>(It.IsAny<IEnumerable<Product>>()))
            .Returns(new List<ProductDto> { new() { Id = "p1", Name = "Laptop", CategoryId = "cat-1" } });

        await _handler.Handle(new GetProductsQuery(), CancellationToken.None);

        _cacheMock.Verify(
            c => c.SetAsync(
                It.IsAny<string>(),
                It.IsAny<PagedResultDto<ProductDto>>(),
                It.IsAny<TimeSpan>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_CacheMiss_ShouldReturnPagedResult()
    {
        var dtos = new List<ProductDto> { new() { Id = "p1", Name = "Laptop", CategoryId = "cat-1" } };
        _productRepoMock
            .Setup(r => r.GetPagedAsync(1, 10, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<Product>(), 1));
        _mapperMock
            .Setup(m => m.Map<IEnumerable<ProductDto>>(It.IsAny<IEnumerable<Product>>()))
            .Returns(dtos);

        var result = await _handler.Handle(new GetProductsQuery(1, 10), CancellationToken.None);

        result.TotalRecords.Should().Be(1);
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(10);
    }
}
