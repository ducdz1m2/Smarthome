namespace Domain.Events
{
    public record ProductStockSynchronizedEvent(int ProductId, int Variantid, int StockQuantity) : DomainEvent;
  
}
