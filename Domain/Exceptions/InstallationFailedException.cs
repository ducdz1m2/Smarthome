namespace Domain.Exceptions
{
    public class InstallationFailedException : DomainException
    {
        public int BookingId { get; }
        public string FailureReason { get; }

        public InstallationFailedException(int bookingId, string reason)
            : base($"Lắp đặt #{bookingId} thất bại: {reason}")
        {
            BookingId = bookingId;
            FailureReason = reason;
        }
    }
}