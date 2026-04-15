namespace Application.DTOs.Responses;

public class NotificationResponse
{
    public int Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? ActionUrl { get; set; }
    public string? Icon { get; set; }
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public string TimeAgo { get; set; } = string.Empty;

    // Related entity info
    public int? RelatedEntityId { get; set; }
    public string? RelatedEntityType { get; set; }
}

public class NotificationCountResponse
{
    public int TotalUnread { get; set; }
    public int OrderUnread { get; set; }
    public int InstallationUnread { get; set; }
    public int WarrantyUnread { get; set; }
    public int ChatUnread { get; set; }
    public int SystemUnread { get; set; }
}
