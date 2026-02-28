namespace Hypesoft.Domain.DomainEvents.Categories;

public sealed record CategoryUpdatedEvent(string CategoryId, string Name, string? Description) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
