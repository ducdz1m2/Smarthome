namespace Domain.Events
{
    public class ProductCreatedEvent : DomainEvent
    {
        public int ProductId { get; }

        public ProductCreatedEvent(int productId)
        {
            ProductId = productId;
        }
    }
}
