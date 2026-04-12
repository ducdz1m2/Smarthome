namespace Domain.Abstractions;

/// <summary>
/// Base class for all Aggregate Roots in the domain.
/// Aggregate Roots are entities that define transaction boundaries
/// and are responsible for maintaining invariants within their aggregate.
/// </summary>
public abstract class AggregateRoot : Entity
{
    private readonly List<INotification> _domainEvents = new();

    protected AggregateRoot() : base() { }

    protected AggregateRoot(int id) : base(id) { }

    public new IReadOnlyCollection<INotification> DomainEvents => _domainEvents.AsReadOnly();

    public void AddDomainEvent(INotification domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void RemoveDomainEvent(INotification domainEvent)
    {
        _domainEvents.Remove(domainEvent);
    }

    public new void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}

/// <summary>
/// Marker interface for domain events
/// </summary>
public interface INotification
{
    Guid EventId { get; }
    DateTime OccurredOn { get; }
}
