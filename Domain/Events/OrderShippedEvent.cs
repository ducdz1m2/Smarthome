namespace Domain.Events
{
    public class OrderShippedEvent : DomainEvent
    {
        public int OrderId { get; }
        public DateTime ShippedAt { get; }

        public OrderShippedEvent(int orderId, DateTime shippedAt)
        {
            OrderId = orderId;
            ShippedAt = shippedAt;
        }
    }
}
