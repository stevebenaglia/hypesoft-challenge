namespace Hypesoft.Domain.DomainEvents.Products;

public sealed record ProductDeletedEvent(
    string ProductId,
    string Name) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
