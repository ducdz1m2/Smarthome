namespace Domain.Events
{
    public class OrderCompletedEvent : DomainEvent
    {
        public int OrderId { get; }
        public DateTime CompletedAt { get; }

        public OrderCompletedEvent(int orderId, DateTime completedAt)
        {
            OrderId = orderId;
            CompletedAt = completedAt;
        }
    }
}
