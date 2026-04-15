using Domain.Enums;

namespace Application.DTOs.Requests;

public class CreateNotificationRequest
{
    public int UserId { get; set; }
    public UserType UserType { get; set; }
    public NotificationType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? ActionUrl { get; set; }
    public string? Icon { get; set; }
    public int? RelatedEntityId { get; set; }
    public string? RelatedEntityType { get; set; }
}

public class CreateBulkNotificationRequest
{
    public List<int> UserIds { get; set; } = new();
    public UserType UserType { get; set; }
    public NotificationType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? ActionUrl { get; set; }
    public string? Icon { get; set; }
    public int? RelatedEntityId { get; set; }
    public string? RelatedEntityType { get; set; }
}

public class MarkNotificationsReadRequest
{
    public List<int> NotificationIds { get; set; } = new();
}
