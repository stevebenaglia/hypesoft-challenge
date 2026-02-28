namespace Hypesoft.Domain.DomainEvents.Categories;

public sealed record CategoryDeletedEvent(
    string CategoryId,
    string Name) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
