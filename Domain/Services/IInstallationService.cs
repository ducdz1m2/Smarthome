using Domain.Entities.Installation;
using Domain.Enums;

namespace Domain.Services;

/// <summary>
/// Domain service for installation booking and scheduling.
/// </summary>
public interface IInstallationService
{
    /// <summary>
    /// Check if a product requires installation.
    /// </summary>
    Task<bool> ProductRequiresInstallationAsync(
        int productId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get available installation slots for a technician.
    /// </summary>
    Task<IReadOnlyList<InstallationSlot>> GetAvailableSlotsAsync(
        int technicianId,
        DateTime date,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if a technician is available for a specific time slot.
    /// </summary>
    Task<bool> IsTechnicianAvailableAsync(
        int technicianId,
        DateTime date,
        TimeSpan startTime,
        TimeSpan duration,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Find the best available technician for an installation.
    /// </summary>
    Task<TechnicianProfile?> FindAvailableTechnicianAsync(
        DateTime preferredDate,
        string? district = null,
        string? requiredSkill = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Schedule an installation for an order.
    /// </summary>
    Task<InstallationBooking> ScheduleInstallationAsync(
        int orderId,
        int technicianId,
        DateTime scheduledDate,
        TimeSpan startTime,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Reschedule an existing installation.
    /// </summary>
    Task<InstallationBooking> RescheduleInstallationAsync(
        int bookingId,
        DateTime newDate,
        TimeSpan newStartTime,
        string reason,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancel an installation booking.
    /// </summary>
    Task<bool> CancelInstallationAsync(
        int bookingId,
        string reason,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Complete an installation.
    /// </summary>
    Task<bool> CompleteInstallationAsync(
        int bookingId,
        string? completionNotes = null,
        IEnumerable<InstallationMaterial>? materialsUsed = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get installation history for a technician.
    /// </summary>
    Task<IReadOnlyList<InstallationBooking>> GetTechnicianHistoryAsync(
        int technicianId,
        DateTime fromDate,
        DateTime toDate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Calculate installation fee for products.
    /// </summary>
    Task<decimal> CalculateInstallationFeeAsync(
        IEnumerable<int> productIds,
        string? district = null,
        CancellationToken cancellationToken = default);
}
