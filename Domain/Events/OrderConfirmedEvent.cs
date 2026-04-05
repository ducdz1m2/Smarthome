namespace Domain.Events
{
    public class OrderConfirmedEvent : DomainEvent
    {
        public int OrderId { get; }
        public DateTime ConfirmAt { get; }

        public OrderConfirmedEvent(int orderId, DateTime confirmAt)
        {
            OrderId = orderId;
            ConfirmAt = confirmAt;
        }
    }
}
