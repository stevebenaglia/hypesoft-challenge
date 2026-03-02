using AutoMapper;
using FluentAssertions;
using Hypesoft.Application.Commands.Categories;
using Hypesoft.Application.DomainEvents;
using Hypesoft.Application.DTOs;
using Hypesoft.Application.Handlers.Categories;
using Hypesoft.Application.Interfaces;
using Hypesoft.Domain.DomainEvents.Categories;
using Hypesoft.Domain.Entities;
using Hypesoft.Domain.Exceptions;
using Hypesoft.Domain.Repositories;
using MediatR;
using Moq;

namespace Hypesoft.UnitTests.Handlers.Categories;

public sealed class UpdateCategoryHandlerTests
{
    private readonly Mock<ICategoryRepository> _categoryRepoMock = new();
    private readonly Mock<IMapper> _mapperMock = new();
    private readonly Mock<IPublisher> _publisherMock = new();
    private readonly Mock<ICacheService> _cacheMock = new();
    private readonly UpdateCategoryHandler _handler;

    public UpdateCategoryHandlerTests()
    {
        _cacheMock
            .Setup(c => c.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _publisherMock
            .Setup(p => p.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _categoryRepoMock
            .Setup(r => r.UpdateAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _handler = new UpdateCategoryHandler(
            _categoryRepoMock.Object,
            _mapperMock.Object,
            _publisherMock.Object,
            _cacheMock.Object);
    }

    [Fact]
    public async Task Handle_CategoryNotFound_ShouldThrowNotFoundException()
    {
        _categoryRepoMock
            .Setup(r => r.GetByIdAsync("nonexistent", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);

        var act = () => _handler.Handle(
            new UpdateCategoryCommand("nonexistent", "New Name", null),
            CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*Category*nonexistent*");
    }

    [Fact]
    public async Task Handle_ValidUpdate_ShouldCallUpdateAsync()
    {
        var category = Category.Create("cat-1", "Electronics", null);
        _categoryRepoMock
            .Setup(r => r.GetByIdAsync("cat-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);
        _mapperMock
            .Setup(m => m.Map<CategoryDto>(It.IsAny<Category>()))
            .Returns(new CategoryDto { Id = "cat-1", Name = "Gadgets" });

        await _handler.Handle(new UpdateCategoryCommand("cat-1", "Gadgets", null), CancellationToken.None);

        _categoryRepoMock.Verify(
            r => r.UpdateAsync(It.Is<Category>(c => c.Name == "Gadgets"), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ValidUpdate_ShouldInvalidateCategoriesCache()
    {
        var category = Category.Create("cat-1", "Electronics", null);
        _categoryRepoMock
            .Setup(r => r.GetByIdAsync("cat-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);
        _mapperMock
            .Setup(m => m.Map<CategoryDto>(It.IsAny<Category>()))
            .Returns(new CategoryDto { Id = "cat-1", Name = "Gadgets" });

        await _handler.Handle(new UpdateCategoryCommand("cat-1", "Gadgets", null), CancellationToken.None);

        _cacheMock.Verify(
            c => c.RemoveAsync(CacheKeys.AllCategories, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ValidUpdate_ShouldPublishCategoryUpdatedEvent()
    {
        var category = Category.Create("cat-1", "Electronics", null);
        _categoryRepoMock
            .Setup(r => r.GetByIdAsync("cat-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);
        _mapperMock
            .Setup(m => m.Map<CategoryDto>(It.IsAny<Category>()))
            .Returns(new CategoryDto { Id = "cat-1", Name = "Gadgets" });

        await _handler.Handle(new UpdateCategoryCommand("cat-1", "Gadgets", null), CancellationToken.None);

        _publisherMock.Verify(
            p => p.Publish(
                It.Is<DomainEventNotification<CategoryUpdatedEvent>>(n =>
                    n.DomainEvent.CategoryId == "cat-1" && n.DomainEvent.Name == "Gadgets"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ValidUpdate_ShouldReturnMappedDto()
    {
        var category = Category.Create("cat-1", "Electronics", null);
        var expected = new CategoryDto { Id = "cat-1", Name = "Gadgets" };
        _categoryRepoMock
            .Setup(r => r.GetByIdAsync("cat-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);
        _mapperMock.Setup(m => m.Map<CategoryDto>(It.IsAny<Category>())).Returns(expected);

        var result = await _handler.Handle(
            new UpdateCategoryCommand("cat-1", "Gadgets", null),
            CancellationToken.None);

        result.Should().Be(expected);
    }
}
