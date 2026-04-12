namespace Domain.Events;

/// <summary>
/// Context information that accompanies domain events.
/// </summary>
public record DomainEventContext
{
    public Guid CorrelationId { get; init; } = Guid.NewGuid();
    public string? TriggeredBy { get; init; }
    public string? ServiceName { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public Dictionary<string, object> Metadata { get; init; } = new();
}
