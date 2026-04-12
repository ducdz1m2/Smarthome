using Domain.Abstractions;

namespace Domain.Events;

/// <summary>
/// Base record for all domain events.
/// Domain events represent business events that have occurred within the domain.
/// </summary>
public abstract record DomainEvent : INotification
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
    public int? EntityId { get; init; }
    public string? EntityType { get; init; }
    public DomainEventContext? Context { get; init; }

    protected DomainEvent() { }

    protected DomainEvent(int entityId, string entityType, DomainEventContext? context = null)
    {
        EntityId = entityId;
        EntityType = entityType;
        Context = context;
    }
}

/// <summary>
/// Base interface for domain event handlers discovery.
/// </summary>
public interface IDomainEvent
{
    Guid EventId { get; }
    DateTime OccurredOn { get; }
}
