namespace Domain.Exceptions;

/// <summary>
/// Exception thrown when installation booking fails.
/// </summary>
public class InstallationBookingException : DomainException
{
    public int? OrderId { get; }

    public InstallationBookingException(int orderId, string reason)
        : base("InstallationBookingFailed", $"Cannot book installation for order {orderId}: {reason}")
    {
        OrderId = orderId;
    }

    public InstallationBookingException(string reason)
        : base("InstallationBookingFailed", reason)
    {
    }
}

/// <summary>
/// Exception thrown when a technician is not available.
/// </summary>
public class TechnicianNotAvailableException : DomainException
{
    public int TechnicianId { get; }
    public DateTime RequestedDate { get; }

    public TechnicianNotAvailableException(int technicianId, DateTime requestedDate)
        : base("TechnicianNotAvailable", $"Technician {technicianId} is not available on {requestedDate:yyyy-MM-dd}.")
    {
        TechnicianId = technicianId;
        RequestedDate = requestedDate;
    }
}

/// <summary>
/// Exception thrown when an installation fails.
/// </summary>
public class InstallationFailedException : DomainException
{
    public int BookingId { get; }
    public string FailureReason { get; }

    public InstallationFailedException(int bookingId, string failureReason)
        : base("InstallationFailed", $"Installation {bookingId} failed: {failureReason}")
    {
        BookingId = bookingId;
        FailureReason = failureReason;
    }
}

/// <summary>
/// Exception thrown when an installation slot is outside working hours.
/// </summary>
public class InvalidInstallationSlotException : DomainException
{
    public DateTime RequestedSlot { get; }

    public InvalidInstallationSlotException(DateTime requestedSlot, string reason)
        : base("InvalidInstallationSlot", $"Installation slot {requestedSlot:yyyy-MM-dd HH:mm} is invalid: {reason}")
    {
        RequestedSlot = requestedSlot;
    }
}
