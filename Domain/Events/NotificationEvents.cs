using Domain.Enums;

namespace Domain.Events;

// Notification Events - these will be handled to send push notifications
public record NotificationCreatedEvent(
    int NotificationId,
    int UserId,
    UserType UserType,
    NotificationType Type,
    string Title,
    string Message,
    string? ActionUrl = null) : DomainEvent;

public record NotificationSentEvent(int NotificationId, int UserId, DateTime SentAt) : DomainEvent;

public record NotificationReadEvent(int NotificationId, int UserId, DateTime ReadAt) : DomainEvent;

public record BulkNotificationCreatedEvent(
    List<int> NotificationIds,
    List<int>? UserIds,
    UserType UserType,
    NotificationType Type,
    string Title,
    string Message,
    string? ActionUrl = null) : DomainEvent;
