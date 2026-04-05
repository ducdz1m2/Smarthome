namespace Domain.Events
{
    public class InstallationCompletedEvent : DomainEvent
    {
        public int BookingId { get; }
        public int OrderId { get; }
        public int TechnicalId { get; }
        public int CustomerRating  { get; }

        public InstallationCompletedEvent(int bookingId, int orderId, int technicianId, int customerRating)
        {
            BookingId = bookingId;
            OrderId = orderId;
            TechnicalId = technicianId;
            CustomerRating = customerRating;
        }
    }
}
