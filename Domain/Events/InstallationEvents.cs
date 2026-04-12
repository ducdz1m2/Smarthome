namespace Domain.Events;

// Installation Aggregate Events

public record InstallationBookingCreatedEvent(
    int BookingId,
    int OrderId,
    int TechnicianId,
    DateTime ScheduledDate) : DomainEvent(BookingId, nameof(Entities.Installation.InstallationBooking));

public record InstallationBookingConfirmedEvent(
    int BookingId,
    DateTime ConfirmedAt) : DomainEvent(BookingId, nameof(Entities.Installation.InstallationBooking));

public record InstallationStartedEvent(
    int BookingId,
    DateTime StartedAt) : DomainEvent(BookingId, nameof(Entities.Installation.InstallationBooking));

public record InstallationCompletedEvent(
    int BookingId,
    DateTime CompletedAt,
    string? Notes) : DomainEvent(BookingId, nameof(Entities.Installation.InstallationBooking));

public record InstallationCancelledEvent(
    int BookingId,
    string Reason) : DomainEvent(BookingId, nameof(Entities.Installation.InstallationBooking));

public record TechnicianAssignedEvent(
    int BookingId,
    int TechnicianId,
    DateTime AssignedAt) : DomainEvent(BookingId, nameof(Entities.Installation.InstallationBooking));
