namespace Domain.Events;

/// <summary>
/// Interface for dispatching domain events.
/// Implementation should be in Infrastructure layer.
/// </summary>
public interface IDomainEventDispatcher
{
    Task DispatchAsync(IEnumerable<DomainEvent> events, CancellationToken cancellationToken = default);
    Task DispatchAsync(DomainEvent domainEvent, CancellationToken cancellationToken = default);
}

/// <summary>
/// Interface for handling domain events.
/// </summary>
public interface IDomainEventHandler<in TEvent> where TEvent : DomainEvent
{
    Task HandleAsync(TEvent domainEvent, CancellationToken cancellationToken = default);
}
