namespace Domain.Events
{
    public class OrderCreatedEvent : DomainEvent
    {
        public int OrderId { get; }
        public int UserId { get; }
        public decimal TotalAmount { get; }

        public OrderCreatedEvent(int orderId, int userId, decimal totalAmount)
        {
            OrderId = orderId;
            UserId = userId;
            TotalAmount = totalAmount;
        }
    }
}
