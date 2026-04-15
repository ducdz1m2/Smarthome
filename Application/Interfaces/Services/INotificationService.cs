using Application.DTOs.Requests;
using Application.DTOs.Responses;
using Domain.Enums;

namespace Application.Interfaces.Services;

public interface INotificationService
{
    // Query methods
    Task<List<NotificationResponse>> GetUserNotificationsAsync(int userId, UserType userType, int limit = 20);
    Task<List<NotificationResponse>> GetUnreadNotificationsAsync(int userId, UserType userType);
    Task<int> GetUnreadCountAsync(int userId, UserType userType);
    Task<int> GetUnreadCountByTypeAsync(int userId, UserType userType, NotificationType type);
    Task<NotificationResponse?> GetByIdAsync(int id, int userId, UserType userType);

    // Create notifications
    Task<int> CreateNotificationAsync(CreateNotificationRequest request);
    Task CreateBulkNotificationsAsync(CreateBulkNotificationRequest request);

    // Mark as read/unread
    Task MarkAsReadAsync(int notificationId, int userId);
    Task MarkAllAsReadAsync(int userId, UserType userType);
    Task MarkAsUnreadAsync(int notificationId, int userId);

    // Delete
    Task DeleteAsync(int notificationId, int userId);
    Task DeleteAllReadAsync(int userId, UserType userType);

    // Push notification delivery status
    Task MarkAsSentAsync(int notificationId);
    Task MarkSendFailedAsync(int notificationId, string error);

    // Event-driven notification creation helpers
    Task NotifyOrderStatusChangedAsync(int orderId, int userId, OrderStatus oldStatus, OrderStatus newStatus);
    Task NotifyInstallationStatusChangedAsync(int installationId, int userId, string status);
    Task NotifyWarrantyClaimUpdatedAsync(int claimId, int userId, string status);
    Task NotifyNewMessageAsync(int chatRoomId, int senderId, int recipientId, UserType recipientType, string message);
}
