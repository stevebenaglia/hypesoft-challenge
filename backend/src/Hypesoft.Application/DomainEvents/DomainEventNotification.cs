using Hypesoft.Domain.DomainEvents;
using MediatR;

namespace Hypesoft.Application.DomainEvents;

/// <summary>
/// MediatR notification wrapper for domain events.
/// Allows Domain events (pure records with no external dependency)
/// to be published through the MediatR pipeline.
/// </summary>
public sealed record DomainEventNotification<TEvent>(TEvent DomainEvent)
    : INotification where TEvent : IDomainEvent;
