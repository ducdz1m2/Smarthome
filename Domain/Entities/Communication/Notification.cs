using Domain.Abstractions;
using Domain.Enums;

namespace Domain.Entities.Communication;

public class Notification : Entity
{
    public int UserId { get; private set; }
    public UserType UserType { get; private set; }
    public NotificationType Type { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Message { get; private set; } = string.Empty;
    public string? ActionUrl { get; private set; }
    public string? Icon { get; private set; }
    public bool IsRead { get; private set; }
    public DateTime? ReadAt { get; private set; }
    public bool IsSent { get; private set; }
    public DateTime? SentAt { get; private set; }
    public new DateTime CreatedAt { get; private set; }
    public string? SendError { get; private set; }

    // Reference data
    public int? RelatedEntityId { get; private set; }
    public string? RelatedEntityType { get; private set; }

    private Notification() { }

    public static Notification Create(
        int userId,
        UserType userType,
        NotificationType type,
        string title,
        string message,
        string? actionUrl = null,
        string? icon = null,
        int? relatedEntityId = null,
        string? relatedEntityType = null)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty");
        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("Message cannot be empty");

        return new Notification
        {
            UserId = userId,
            UserType = userType,
            Type = type,
            Title = title.Trim(),
            Message = message.Trim(),
            ActionUrl = actionUrl,
            Icon = icon,
            RelatedEntityId = relatedEntityId,
            RelatedEntityType = relatedEntityType,
            IsRead = false,
            IsSent = false,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void MarkAsRead()
    {
        if (IsRead) return;

        IsRead = true;
        ReadAt = DateTime.UtcNow;
    }

    public void MarkAsSent()
    {
        IsSent = true;
        SentAt = DateTime.UtcNow;
        SendError = null;
    }

    public void MarkSendFailed(string error)
    {
        SendError = error;
    }

    public void Unread()
    {
        IsRead = false;
        ReadAt = null;
    }
}
