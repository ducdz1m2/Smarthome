namespace Application.DTOs.Responses;

public class ChatRoomResponse
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // OneToOne, Support, Group
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ClosedAt { get; set; }

    // Related entities
    public int? RelatedOrderId { get; set; }
    public int? RelatedInstallationId { get; set; }
    public int? RelatedWarrantyClaimId { get; set; }

    // Participants
    public List<ChatParticipantResponse> Participants { get; set; } = new();

    // Last message info
    public ChatMessageSummaryResponse? LastMessage { get; set; }

    // Unread count for current user
    public int UnreadCount { get; set; }
}

public class ChatParticipantResponse
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string UserType { get; set; } = string.Empty; // Customer, Admin, Technician
    public string? UserName { get; set; }
    public string? UserAvatar { get; set; }
    public DateTime JoinedAt { get; set; }
    public bool IsActive { get; set; }
    public bool IsBlocked { get; set; }
    public int UnreadCount { get; set; }
    public DateTime? LastActivityAt { get; set; }
}

public class ChatMessageResponse
{
    public int Id { get; set; }
    public int ChatRoomId { get; set; }
    public int SenderId { get; set; }
    public string SenderType { get; set; } = string.Empty;
    public string? SenderName { get; set; }
    public string? SenderAvatar { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }
    public DateTime? EditedAt { get; set; }
    public bool IsDeleted { get; set; }
    public bool IsFromMe { get; set; }
    public List<ChatAttachmentResponse> Attachments { get; set; } = new();
}

public class ChatMessageSummaryResponse
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public int SenderId { get; set; }
    public string SenderType { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }
}

public class ChatAttachmentResponse
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public string? FileType { get; set; }
    public long? FileSize { get; set; }
    public string? FileSizeFormatted { get; set; }
}
