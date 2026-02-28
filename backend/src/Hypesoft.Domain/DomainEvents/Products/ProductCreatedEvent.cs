namespace Hypesoft.Domain.DomainEvents.Products;

public sealed record ProductCreatedEvent(
    string ProductId,
    string Name,
    decimal Price,
    int StockQuantity,
    string CategoryId) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
