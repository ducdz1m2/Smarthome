namespace Domain.Events
{
    public class ProductPriceChangedEvent : DomainEvent
    {
        public int ProductId { get; }
        public decimal OldPrice { get; }
        public decimal NewPrice { get; }

        public ProductPriceChangedEvent(int productId, decimal oldPrice, decimal newPrice)
        {
            ProductId = productId;
            OldPrice = oldPrice;
            NewPrice = newPrice;
        }
    }
}
