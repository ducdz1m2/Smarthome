using Application.Interfaces.Services;
using Domain.Events;
using Microsoft.AspNetCore.SignalR;
using Web.Hubs;

namespace Web.EventHandlers;

public class PushNotificationHandler :
    IDomainEventHandler<NotificationCreatedEvent>,
    IDomainEventHandler<BulkNotificationCreatedEvent>
{
    private readonly IHubContext<NotificationHub> _hubContext;

    public PushNotificationHandler(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task HandleAsync(NotificationCreatedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"[PushNotificationHandler] HandleAsync called for notification {domainEvent.NotificationId}");
        
        // Send notification to the specific user's SignalR group
        var groupName = $"user_{domainEvent.UserId}";
        
        try
        {
            await _hubContext.Clients.Group(groupName).SendAsync("ReceiveNotification", new
            {
                Id = domainEvent.NotificationId,
                UserId = domainEvent.UserId,
                UserType = domainEvent.UserType.ToString(),
                Type = domainEvent.Type.ToString(),
                Title = domainEvent.Title,
                Message = domainEvent.Message,
                ActionUrl = domainEvent.ActionUrl,
                CreatedAt = DateTime.UtcNow
            }, cancellationToken);

            Console.WriteLine($"[PushNotificationHandler] Sent notification {domainEvent.NotificationId} to user {domainEvent.UserId} in group {groupName}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[PushNotificationHandler] Error sending notification: {ex.Message}");
            Console.WriteLine($"[PushNotificationHandler] Stack trace: {ex.StackTrace}");
        }
    }

    public async Task HandleAsync(BulkNotificationCreatedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"[PushNotificationHandler] Bulk notification handler called for {domainEvent.UserIds.Count} users, UserType: {domainEvent.UserType}");
        
        var notificationData = new
        {
            NotificationIds = domainEvent.NotificationIds,
            Type = domainEvent.Type.ToString(),
            Title = domainEvent.Title,
            Message = domainEvent.Message,
            ActionUrl = domainEvent.ActionUrl,
            CreatedAt = DateTime.UtcNow
        };

        try
        {
            // If it's a bulk notification for admins, send to admin_notif group
            if (domainEvent.UserType == Domain.Enums.UserType.Admin)
            {
                await _hubContext.Clients.Group("admin_notif").SendAsync("ReceiveNotification", notificationData, cancellationToken);
                Console.WriteLine($"[PushNotificationHandler] Sent bulk notification to admin_notif group");
            }
            else if (domainEvent.UserIds != null && domainEvent.UserIds.Count > 0)
            {
                // Send to specific users
                foreach (var userId in domainEvent.UserIds)
                {
                    var groupName = $"user_{userId}";
                    await _hubContext.Clients.Group(groupName).SendAsync("ReceiveNotification", notificationData, cancellationToken);
                }
                Console.WriteLine($"[PushNotificationHandler] Sent bulk notification to {domainEvent.UserIds.Count} users");
            }
            else
            {
                Console.WriteLine($"[PushNotificationHandler] No users specified for bulk notification");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[PushNotificationHandler] Error sending bulk notification: {ex.Message}");
            Console.WriteLine($"[PushNotificationHandler] Stack trace: {ex.StackTrace}");
        }
    }
}
