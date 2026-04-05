namespace Domain.Events
{
    public class InstallationScheduledEvent : DomainEvent
    {
        public int BookingId { get; } 
        public int OrderId { get; }
        public int TechnicalId { get; }
        public DateTime ScheduledDate { get; }

        public InstallationScheduledEvent(int bookingId, int orderId, int technicianId, DateTime scheduledDate)
        {
            BookingId = bookingId;
            OrderId = orderId;
            TechnicalId = technicianId;
            ScheduledDate = scheduledDate;
        }
    }
}
