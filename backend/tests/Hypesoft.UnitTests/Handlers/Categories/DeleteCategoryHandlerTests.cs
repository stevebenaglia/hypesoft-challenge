using FluentAssertions;
using Hypesoft.Application.Commands.Categories;
using Hypesoft.Application.DomainEvents;
using Hypesoft.Application.Handlers.Categories;
using Hypesoft.Application.Interfaces;
using Hypesoft.Domain.DomainEvents.Categories;
using Hypesoft.Domain.Entities;
using Hypesoft.Domain.Exceptions;
using Hypesoft.Domain.Repositories;
using MediatR;
using Moq;

namespace Hypesoft.UnitTests.Handlers.Categories;

public sealed class DeleteCategoryHandlerTests
{
    private readonly Mock<ICategoryRepository> _categoryRepoMock = new();
    private readonly Mock<IProductRepository> _productRepoMock = new();
    private readonly Mock<IPublisher> _publisherMock = new();
    private readonly Mock<ICacheInvalidationService> _cacheInvalidationMock = new();
    private readonly DeleteCategoryHandler _handler;

    public DeleteCategoryHandlerTests()
    {
        _cacheInvalidationMock
            .Setup(s => s.InvalidateCategoryMutationAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _publisherMock
            .Setup(p => p.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _categoryRepoMock
            .Setup(r => r.DeleteAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _handler = new DeleteCategoryHandler(
            _categoryRepoMock.Object,
            _productRepoMock.Object,
            _publisherMock.Object,
            _cacheInvalidationMock.Object);
    }

    [Fact]
    public async Task Handle_CategoryNotFound_ShouldThrowNotFoundException()
    {
        _categoryRepoMock
            .Setup(r => r.GetByIdAsync("nonexistent", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);

        var act = () => _handler.Handle(new DeleteCategoryCommand("nonexistent"), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*Category*nonexistent*");
    }

    [Fact]
    public async Task Handle_CategoryHasProducts_ShouldThrowDomainException()
    {
        var category = Category.Create("cat-1", "Electronics", null);
        _categoryRepoMock
            .Setup(r => r.GetByIdAsync("cat-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);
        _productRepoMock
            .Setup(r => r.HasProductsInCategoryAsync("cat-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var act = () => _handler.Handle(new DeleteCategoryCommand("cat-1"), CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*cannot be deleted*");
    }

    [Fact]
    public async Task Handle_ValidDelete_ShouldCallDeleteAsync()
    {
        var category = Category.Create("cat-1", "Electronics", null);
        _categoryRepoMock
            .Setup(r => r.GetByIdAsync("cat-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);
        _productRepoMock
            .Setup(r => r.HasProductsInCategoryAsync("cat-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        await _handler.Handle(new DeleteCategoryCommand("cat-1"), CancellationToken.None);

        _categoryRepoMock.Verify(r => r.DeleteAsync(category, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ValidDelete_ShouldInvalidateCategoryMutation()
    {
        var category = Category.Create("cat-1", "Electronics", null);
        _categoryRepoMock
            .Setup(r => r.GetByIdAsync("cat-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);
        _productRepoMock
            .Setup(r => r.HasProductsInCategoryAsync("cat-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        await _handler.Handle(new DeleteCategoryCommand("cat-1"), CancellationToken.None);

        _cacheInvalidationMock.Verify(
            s => s.InvalidateCategoryMutationAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ValidDelete_ShouldPublishCategoryDeletedEvent()
    {
        var category = Category.Create("cat-1", "Electronics", null);
        _categoryRepoMock
            .Setup(r => r.GetByIdAsync("cat-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);
        _productRepoMock
            .Setup(r => r.HasProductsInCategoryAsync("cat-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        await _handler.Handle(new DeleteCategoryCommand("cat-1"), CancellationToken.None);

        _publisherMock.Verify(
            p => p.Publish(
                It.Is<DomainEventNotification<CategoryDeletedEvent>>(n => n.DomainEvent.CategoryId == "cat-1"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
