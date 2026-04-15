using Application.DTOs.Requests;
using Application.Interfaces.Services;
using Domain.Enums;
using Domain.Events;

namespace Application.EventHandlers;

public class InstallationNotificationHandler :
    IDomainEventHandler<InstallationBookedEvent>,
    IDomainEventHandler<InstallationAssignedEvent>,
    IDomainEventHandler<InstallationCompletedEvent>
{
    private readonly INotificationService _notificationService;

    public InstallationNotificationHandler(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    public async Task HandleAsync(InstallationBookedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        // Notify customer
        await _notificationService.CreateNotificationAsync(new CreateNotificationRequest
        {
            UserId = domainEvent.CustomerId,
            UserType = UserType.Customer,
            Type = NotificationType.InstallationScheduled,
            Title = "Lịch lắp đặt đã được đặt",
            Message = "Lịch lắp đặt của bạn đã được tạo. Chúng tôi sẽ thông báo khi có kỹ thuật viên phụ trách.",
            ActionUrl = $"/installations/{domainEvent.BookingId}",
            Icon = "calendar-check",
            RelatedEntityId = domainEvent.BookingId,
            RelatedEntityType = "InstallationBooking"
        });
    }

    public async Task HandleAsync(InstallationAssignedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        // Notify technician
        await _notificationService.CreateNotificationAsync(new CreateNotificationRequest
        {
            UserId = domainEvent.TechnicianId,
            UserType = UserType.Technician,
            Type = NotificationType.InstallationAssigned,
            Title = "Bạn có lịch lắp đặt mới",
            Message = $"Bạn đã được phân công lịch lắp đặt #{domainEvent.BookingId}.",
            ActionUrl = $"/technician/installations/{domainEvent.BookingId}",
            Icon = "user-cog",
            RelatedEntityId = domainEvent.BookingId,
            RelatedEntityType = "InstallationBooking"
        });

        // Notify customer
        await _notificationService.CreateNotificationAsync(new CreateNotificationRequest
        {
            UserId = domainEvent.CustomerId,
            UserType = UserType.Customer,
            Type = NotificationType.InstallationAssigned,
            Title = "Đã phân công kỹ thuật viên",
            Message = "Lịch lắp đặt của bạn đã được phân công kỹ thuật viên. Kỹ thuật viên sẽ liên hệ với bạn để xác nhận.",
            ActionUrl = $"/installations/{domainEvent.BookingId}",
            Icon = "user-check",
            RelatedEntityId = domainEvent.BookingId,
            RelatedEntityType = "InstallationBooking"
        });
    }

    public async Task HandleAsync(InstallationCompletedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        await _notificationService.CreateNotificationAsync(new CreateNotificationRequest
        {
            UserId = domainEvent.CustomerId,
            UserType = UserType.Customer,
            Type = NotificationType.InstallationCompleted,
            Title = "Lắp đặt hoàn thành",
            Message = "Lịch lắp đặt đã hoàn thành. Cảm ơn bạn đã sử dụng dịch vụ của SmartHome!",
            ActionUrl = $"/installations/{domainEvent.BookingId}",
            Icon = "check-circle",
            RelatedEntityId = domainEvent.BookingId,
            RelatedEntityType = "InstallationBooking"
        });
    }
}
