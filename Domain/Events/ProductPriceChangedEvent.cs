namespace Domain.Events
{
    public record ProductPriceChangedEvent(int ProductId, decimal OldPrice, decimal NewPrice) : DomainEvent;
}
