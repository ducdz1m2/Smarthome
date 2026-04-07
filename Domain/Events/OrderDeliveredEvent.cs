namespace Domain.Events
{
    public class OrderDeliveredEvent : DomainEvent
    {
        public int OrderId { get; }
        public DateTime DeliveredAt { get; }

        public OrderDeliveredEvent(int orderId, DateTime deliveredAt)
        {
            OrderId = orderId;
            DeliveredAt = deliveredAt;
        }
    }
}
