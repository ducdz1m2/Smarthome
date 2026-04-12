namespace Domain.Events
{
    public record ProductCreatedEvent(int ProductId) : DomainEvent;
}
