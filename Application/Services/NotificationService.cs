using Application.DTOs.Requests;
using Application.DTOs.Responses;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities.Communication;
using Domain.Enums;
using Domain.Exceptions;
using Domain.Events;

namespace Application.Services;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _notificationRepository;
    private readonly IDomainEventDispatcher _eventDispatcher;

    public NotificationService(
        INotificationRepository notificationRepository,
        IDomainEventDispatcher eventDispatcher)
    {
        _notificationRepository = notificationRepository;
        _eventDispatcher = eventDispatcher;
    }

    public async Task<List<NotificationResponse>> GetUserNotificationsAsync(int userId, UserType userType, int limit = 20)
    {
        var notifications = await _notificationRepository.GetRecentAsync(userId, userType, limit);
        return notifications.Select(MapToResponse).ToList();
    }

    public async Task<List<NotificationResponse>> GetUnreadNotificationsAsync(int userId, UserType userType)
    {
        var notifications = await _notificationRepository.GetUnreadByUserIdAsync(userId, userType);
        return notifications.Select(MapToResponse).ToList();
    }

    public async Task<int> GetUnreadCountAsync(int userId, UserType userType)
    {
        return await _notificationRepository.CountUnreadAsync(userId, userType);
    }

    public async Task<int> GetUnreadCountByTypeAsync(int userId, UserType userType, NotificationType type)
    {
        return await _notificationRepository.CountUnreadByTypeAsync(userId, userType, type);
    }

    public async Task<NotificationResponse?> GetByIdAsync(int id, int userId, UserType userType)
    {
        var notification = await _notificationRepository.GetByIdAsync(id);
        if (notification == null || notification.UserId != userId || notification.UserType != userType)
            return null;

        return MapToResponse(notification);
    }

    public async Task<int> CreateNotificationAsync(CreateNotificationRequest request)
    {
        var notification = Notification.Create(
            request.UserId,
            request.UserType,
            request.Type,
            request.Title,
            request.Message,
            request.ActionUrl,
            request.Icon,
            request.RelatedEntityId,
            request.RelatedEntityType);

        await _notificationRepository.AddAsync(notification);
        await _notificationRepository.SaveChangesAsync();

        // Dispatch event for push notification
        await _eventDispatcher.DispatchAsync(new NotificationCreatedEvent(
            notification.Id,
            notification.UserId,
            notification.UserType,
            notification.Type,
            notification.Title,
            notification.Message,
            notification.ActionUrl));

        return notification.Id;
    }

    public async Task CreateBulkNotificationsAsync(CreateBulkNotificationRequest request)
    {
        Console.WriteLine($"[NotificationService] CreateBulkNotificationsAsync called for UserType: {request.UserType}, UserIds: {request.UserIds?.Count ?? 0}");

        var notifications = new List<Notification>();
        var notificationIds = new List<int>();

        // If UserIds is null or empty, return early
        if (request.UserIds == null || !request.UserIds.Any())
        {
            Console.WriteLine($"[NotificationService] UserIds is null or empty, returning early");
            return;
        }

        foreach (var userId in request.UserIds)
        {
            var notification = Notification.Create(
                userId,
                request.UserType,
                request.Type,
                request.Title,
                request.Message,
                request.ActionUrl,
                request.Icon,
                request.RelatedEntityId,
                request.RelatedEntityType);

            notifications.Add(notification);
        }

        await _notificationRepository.AddRangeAsync(notifications);
        await _notificationRepository.SaveChangesAsync();

        notificationIds = notifications.Select(n => n.Id).ToList();

        Console.WriteLine($"[NotificationService] Created {notificationIds.Count} notifications with IDs: {string.Join(", ", notificationIds)}");

        // Dispatch bulk event
        await _eventDispatcher.DispatchAsync(new BulkNotificationCreatedEvent(
            notificationIds,
            request.UserIds,
            request.UserType,
            request.Type,
            request.Title,
            request.Message,
            request.ActionUrl));

        Console.WriteLine($"[NotificationService] BulkNotificationCreatedEvent dispatched");
    }

    public async Task MarkAsReadAsync(int notificationId, int userId)
    {
        var notification = await _notificationRepository.GetByIdAsync(notificationId);
        if (notification == null || notification.UserId != userId)
            throw new DomainException("Không tìm thấy thông báo");

        notification.MarkAsRead();
        _notificationRepository.Update(notification);
        await _notificationRepository.SaveChangesAsync();

        await _eventDispatcher.DispatchAsync(new NotificationReadEvent(
            notificationId,
            userId,
            notification.ReadAt!.Value));
    }

    public async Task MarkAllAsReadAsync(int userId, UserType userType)
    {
        var notifications = await _notificationRepository.GetUnreadByUserIdAsync(userId, userType);

        foreach (var notification in notifications)
        {
            notification.MarkAsRead();
        }

        _notificationRepository.UpdateRange(notifications);
        await _notificationRepository.SaveChangesAsync();
    }

    public async Task MarkAsUnreadAsync(int notificationId, int userId)
    {
        var notification = await _notificationRepository.GetByIdAsync(notificationId);
        if (notification == null || notification.UserId != userId)
            throw new DomainException("Không tìm thấy thông báo");

        notification.Unread();
        _notificationRepository.Update(notification);
        await _notificationRepository.SaveChangesAsync();
    }

    public async Task DeleteAsync(int notificationId, int userId)
    {
        var notification = await _notificationRepository.GetByIdAsync(notificationId);
        if (notification == null || notification.UserId != userId)
            throw new DomainException("Không tìm thấy thông báo");

        _notificationRepository.Delete(notification);
        await _notificationRepository.SaveChangesAsync();
    }

    public async Task DeleteAllReadAsync(int userId, UserType userType)
    {
        var notifications = await _notificationRepository.GetByUserIdAsync(userId, userType);
        var readNotifications = notifications.Where(n => n.IsRead).ToList();

        foreach (var notification in readNotifications)
        {
            _notificationRepository.Delete(notification);
        }

        await _notificationRepository.SaveChangesAsync();
    }

    public async Task MarkAsSentAsync(int notificationId)
    {
        var notification = await _notificationRepository.GetByIdAsync(notificationId);
        if (notification == null) return;

        notification.MarkAsSent();
        _notificationRepository.Update(notification);
        await _notificationRepository.SaveChangesAsync();

        await _eventDispatcher.DispatchAsync(new NotificationSentEvent(
            notificationId,
            notification.UserId,
            notification.SentAt!.Value));
    }

    public async Task MarkSendFailedAsync(int notificationId, string error)
    {
        var notification = await _notificationRepository.GetByIdAsync(notificationId);
        if (notification == null) return;

        notification.MarkSendFailed(error);
        _notificationRepository.Update(notification);
        await _notificationRepository.SaveChangesAsync();
    }

    // Event-driven notification helpers
    public async Task NotifyOrderStatusChangedAsync(int orderId, int userId, OrderStatus oldStatus, OrderStatus newStatus)
    {
        string title, message, actionUrl;

        switch (newStatus)
        {
            case OrderStatus.Confirmed:
                title = "Đơn hàng đã được xác nhận";
                message = $"Đơn hàng #{orderId} của bạn đã được xác nhận và đang được chuẩn bị.";
                break;
            case OrderStatus.Shipping:
                title = "Đơn hàng đang giao";
                message = $"Đơn hàng #{orderId} đang được giao đến bạn.";
                break;
            case OrderStatus.Delivered:
                title = "Đơn hàng đã giao thành công";
                message = $"Đơn hàng #{orderId} đã được giao thành công. Cảm ơn bạn đã mua hàng!";
                break;
            case OrderStatus.Cancelled:
                title = "Đơn hàng đã bị hủy";
                message = $"Đơn hàng #{orderId} đã bị hủy.";
                break;
            default:
                return;
        }

        actionUrl = $"/orders/{orderId}";

        await CreateNotificationAsync(new CreateNotificationRequest
        {
            UserId = userId,
            UserType = UserType.Customer,
            Type = NotificationType.OrderConfirmed,
            Title = title,
            Message = message,
            ActionUrl = actionUrl,
            Icon = "shopping-cart",
            RelatedEntityId = orderId,
            RelatedEntityType = "Order"
        });
    }

    public async Task NotifyInstallationStatusChangedAsync(int installationId, int userId, string status)
    {
        string title, message, icon;

        switch (status.ToLower())
        {
            case "assigned":
                title = "Đã phân công kỹ thuật viên";
                message = "Lịch lắp đặt của bạn đã được phân công kỹ thuật viên.";
                icon = "user-cog";
                break;
            case "installing":
                title = "Đang lắp đặt";
                message = "Kỹ thuật viên đang tiến hành lắp đặt.";
                icon = "tools";
                break;
            case "completed":
                title = "Lắp đặt hoàn thành";
                message = "Lịch lắp đặt đã hoàn thành. Cảm ơn bạn!";
                icon = "check-circle";
                break;
            default:
                return;
        }

        await CreateNotificationAsync(new CreateNotificationRequest
        {
            UserId = userId,
            UserType = UserType.Customer,
            Type = NotificationType.InstallationScheduled,
            Title = title,
            Message = message,
            ActionUrl = $"/installations/{installationId}",
            Icon = icon,
            RelatedEntityId = installationId,
            RelatedEntityType = "InstallationBooking"
        });
    }

    public async Task NotifyWarrantyClaimUpdatedAsync(int claimId, int userId, string status)
    {
        string title, message;

        switch (status.ToLower())
        {
            case "approved":
                title = "Yêu cầu bảo hành được chấp thuận";
                message = "Yêu cầu bảo hành của bạn đã được chấp thuận.";
                break;
            case "rejected":
                title = "Yêu cầu bảo hành bị từ chối";
                message = "Yêu cầu bảo hành của bạn đã bị từ chối.";
                break;
            case "resolved":
                title = "Yêu cầu bảo hành đã hoàn thành";
                message = "Yêu cầu bảo hành của bạn đã được xử lý xong.";
                break;
            default:
                return;
        }

        await CreateNotificationAsync(new CreateNotificationRequest
        {
            UserId = userId,
            UserType = UserType.Customer,
            Type = NotificationType.WarrantyClaimUpdated,
            Title = title,
            Message = message,
            ActionUrl = $"/warranties/claims/{claimId}",
            Icon = "shield-alt",
            RelatedEntityId = claimId,
            RelatedEntityType = "WarrantyClaim"
        });
    }

    public async Task NotifyNewMessageAsync(int chatRoomId, int senderId, int recipientId, UserType recipientType, string message)
    {
        await CreateNotificationAsync(new CreateNotificationRequest
        {
            UserId = recipientId,
            UserType = recipientType,
            Type = NotificationType.NewMessage,
            Title = "Tin nhắn mới",
            Message = message.Length > 50 ? message[..50] + "..." : message,
            ActionUrl = $"/chat/{chatRoomId}",
            Icon = "comment",
            RelatedEntityId = chatRoomId,
            RelatedEntityType = "ChatRoom"
        });
    }

    private static NotificationResponse MapToResponse(Notification notification)
    {
        return new NotificationResponse
        {
            Id = notification.Id,
            Type = notification.Type.ToString(),
            Title = notification.Title,
            Message = notification.Message,
            ActionUrl = notification.ActionUrl,
            Icon = notification.Icon,
            IsRead = notification.IsRead,
            ReadAt = notification.ReadAt,
            CreatedAt = notification.CreatedAt,
            TimeAgo = FormatTimeAgo(notification.CreatedAt),
            RelatedEntityId = notification.RelatedEntityId,
            RelatedEntityType = notification.RelatedEntityType
        };
    }

    private static string FormatTimeAgo(DateTime dateTime)
    {
        var span = DateTime.UtcNow - dateTime;

        if (span.TotalMinutes < 1)
            return "Vừa xong";
        if (span.TotalHours < 1)
            return $"{(int)span.TotalMinutes} phút trước";
        if (span.TotalDays < 1)
            return $"{(int)span.TotalHours} giờ trước";
        if (span.TotalDays < 7)
            return $"{(int)span.TotalDays} ngày trước";
        return dateTime.ToString("dd/MM/yyyy");
    }
}
