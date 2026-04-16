namespace Application.DTOs.Responses;

public class ChatMessageDto
{
    public int Id { get; set; }
    public string SessionId { get; set; } = string.Empty;
    public int? UserId { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public bool IsFromAdmin { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedDate { get; set; }

    // File attachment fields
    public string? FileUrl { get; set; }
    public string? FileName { get; set; }
    public string? FileType { get; set; }
    public long? FileSize { get; set; }
    public MessageType MessageType { get; set; }
}

public class ChatSessionDto
{
    public string SessionId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public int? UserId { get; set; }
    public string LastMessage { get; set; } = string.Empty;
    public DateTime LastMessageTime { get; set; }
    public int UnreadCount { get; set; }
}

public class PagedMessagesDto
{
    public List<ChatMessageDto> Messages { get; set; } = new();
    public bool HasMore { get; set; }
    public int TotalCount { get; set; }
}

public class FileUploadResult
{
    public string FileUrl { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public bool IsImage { get; set; }
}

public enum MessageType
{
    Text = 0,
    Image = 1,
    File = 2
}
