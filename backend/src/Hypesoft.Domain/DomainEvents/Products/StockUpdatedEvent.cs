namespace Hypesoft.Domain.DomainEvents.Products;

public sealed record StockUpdatedEvent(
    string ProductId,
    int PreviousQuantity,
    int NewQuantity) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
