using AutoMapper;
using FluentAssertions;
using Hypesoft.Application.Commands.Categories;
using Hypesoft.Application.DomainEvents;
using Hypesoft.Application.DTOs;
using Hypesoft.Application.Handlers.Categories;
using Hypesoft.Application.Interfaces;
using Hypesoft.Domain.DomainEvents.Categories;
using Hypesoft.Domain.Entities;
using Hypesoft.Domain.Repositories;
using Hypesoft.Domain.Services;
using MediatR;
using Moq;

namespace Hypesoft.UnitTests.Handlers.Categories;

public sealed class CreateCategoryHandlerTests
{
    private readonly Mock<ICategoryRepository> _categoryRepoMock = new();
    private readonly Mock<IIdGenerator> _idGeneratorMock = new();
    private readonly Mock<IMapper> _mapperMock = new();
    private readonly Mock<IPublisher> _publisherMock = new();
    private readonly Mock<ICacheInvalidationService> _cacheInvalidationMock = new();
    private readonly CreateCategoryHandler _handler;

    public CreateCategoryHandlerTests()
    {
        _idGeneratorMock.Setup(g => g.NewId()).Returns("cat-1");
        _categoryRepoMock
            .Setup(r => r.AddAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()))
            .Returns((Category c, CancellationToken _) => Task.FromResult(c));
        _cacheInvalidationMock
            .Setup(s => s.InvalidateCategoryMutationAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _publisherMock
            .Setup(p => p.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _handler = new CreateCategoryHandler(
            _categoryRepoMock.Object,
            _idGeneratorMock.Object,
            _mapperMock.Object,
            _publisherMock.Object,
            _cacheInvalidationMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldCallAddAsync()
    {
        var dto = new CategoryDto { Id = "cat-1", Name = "Electronics" };
        _mapperMock
            .Setup(m => m.Map<CategoryDto>(It.IsAny<Category>()))
            .Returns(dto);

        var command = new CreateCategoryCommand("Electronics", null);

        await _handler.Handle(command, CancellationToken.None);

        _categoryRepoMock.Verify(
            r => r.AddAsync(It.Is<Category>(c => c.Name == "Electronics"), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldInvalidateCategoryMutation()
    {
        var dto = new CategoryDto { Id = "cat-1", Name = "Electronics" };
        _mapperMock.Setup(m => m.Map<CategoryDto>(It.IsAny<Category>())).Returns(dto);

        await _handler.Handle(new CreateCategoryCommand("Electronics", null), CancellationToken.None);

        _cacheInvalidationMock.Verify(
            s => s.InvalidateCategoryMutationAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldPublishCategoryCreatedEvent()
    {
        var dto = new CategoryDto { Id = "cat-1", Name = "Electronics" };
        _mapperMock.Setup(m => m.Map<CategoryDto>(It.IsAny<Category>())).Returns(dto);

        await _handler.Handle(new CreateCategoryCommand("Electronics", null), CancellationToken.None);

        _publisherMock.Verify(
            p => p.Publish(
                It.Is<DomainEventNotification<CategoryCreatedEvent>>(n => n.DomainEvent.Name == "Electronics"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldReturnMappedDto()
    {
        var expected = new CategoryDto { Id = "cat-1", Name = "Electronics", Description = null };
        _mapperMock.Setup(m => m.Map<CategoryDto>(It.IsAny<Category>())).Returns(expected);

        var result = await _handler.Handle(new CreateCategoryCommand("Electronics", null), CancellationToken.None);

        result.Should().Be(expected);
    }
}
