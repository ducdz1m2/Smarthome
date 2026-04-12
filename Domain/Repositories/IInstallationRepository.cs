using Domain.Entities.Installation;
using Domain.Enums;

namespace Domain.Repositories;

/// <summary>
/// Repository interface for InstallationBooking aggregate.
/// </summary>
public interface IInstallationBookingRepository : IRepository<InstallationBooking>
{
    Task<InstallationBooking?> GetByIdWithDetailsAsync(int id, CancellationToken cancellationToken = default);
    Task<InstallationBooking?> GetByOrderAsync(int orderId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<InstallationBooking>> GetByTechnicianAsync(int technicianId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<InstallationBooking>> GetByTechnicianAndDateRangeAsync(
        int technicianId,
        DateTime fromDate,
        DateTime toDate,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<InstallationBooking>> GetByStatusAsync(InstallationStatus status, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<InstallationBooking>> GetPendingAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<InstallationBooking>> GetOverdueAsync(DateTime before, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<InstallationBooking> Items, int TotalCount)> GetPagedAsync(
        int page,
        int pageSize,
        InstallationStatus? status = null,
        int? technicianId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default);
    Task<bool> HasBookingForOrderAsync(int orderId, CancellationToken cancellationToken = default);
    Task<int> CountCompletedByTechnicianAsync(int technicianId, DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository interface for TechnicianProfile aggregate.
/// </summary>
public interface ITechnicianProfileRepository : IRepository<TechnicianProfile>
{
    Task<TechnicianProfile?> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default);
    Task<TechnicianProfile?> GetByIdWithSlotsAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TechnicianProfile>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TechnicianProfile>> GetAvailableForDateAsync(DateTime date, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TechnicianProfile>> GetBySkillAsync(string skill, CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository interface for InstallationSlot entity.
/// </summary>
public interface IInstallationSlotRepository : IRepository<InstallationSlot>
{
    Task<IReadOnlyList<InstallationSlot>> GetByTechnicianAsync(int technicianId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<InstallationSlot>> GetByTechnicianAndDateAsync(int technicianId, DateTime date, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<InstallationSlot>> GetAvailableSlotsAsync(int technicianId, DateTime date, CancellationToken cancellationToken = default);
    Task<InstallationSlot?> GetByTechnicianAndDateTimeAsync(int technicianId, DateTime date, TimeSpan time, CancellationToken cancellationToken = default);
    Task<bool> IsSlotAvailableAsync(int technicianId, DateTime date, TimeSpan startTime, TimeSpan endTime, CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository interface for InstallationMaterial entity.
/// </summary>
public interface IInstallationMaterialRepository : IRepository<InstallationMaterial>
{
    Task<IReadOnlyList<InstallationMaterial>> GetByBookingAsync(int bookingId, CancellationToken cancellationToken = default);
    Task<decimal> GetTotalCostByBookingAsync(int bookingId, CancellationToken cancellationToken = default);
}
