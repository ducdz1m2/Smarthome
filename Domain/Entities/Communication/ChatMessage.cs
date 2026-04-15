using Domain.Abstractions;
using Domain.Enums;

namespace Domain.Entities.Communication;

public class ChatMessage : Entity
{
    public int ChatRoomId { get; private set; }
    public int SenderId { get; private set; }
    public UserType SenderType { get; private set; }
    public string Content { get; private set; } = string.Empty;
    public DateTime SentAt { get; private set; }
    public DateTime? EditedAt { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }
    public string? DeletedBy { get; private set; }

    private readonly List<ChatAttachment> _attachments = new();
    public IReadOnlyCollection<ChatAttachment> Attachments => _attachments.AsReadOnly();

    public virtual ChatRoom ChatRoom { get; private set; } = null!;

    private ChatMessage() { }

    public static ChatMessage Create(
        int chatRoomId, 
        int senderId, 
        UserType senderType, 
        string content,
        List<ChatAttachment>? attachments = null)
    {
        if (string.IsNullOrWhiteSpace(content) && (attachments == null || !attachments.Any()))
            throw new ArgumentException("Message must have content or attachments");

        var message = new ChatMessage
        {
            ChatRoomId = chatRoomId,
            SenderId = senderId,
            SenderType = senderType,
            Content = content?.Trim() ?? string.Empty,
            SentAt = DateTime.UtcNow,
            IsDeleted = false
        };

        if (attachments != null)
        {
            foreach (var attachment in attachments)
            {
                message.AddAttachment(attachment);
            }
        }

        return message;
    }

    public void Edit(string newContent)
    {
        if (IsDeleted)
            throw new InvalidOperationException("Cannot edit deleted message");

        if (string.IsNullOrWhiteSpace(newContent))
            throw new ArgumentException("Content cannot be empty");

        Content = newContent.Trim();
        EditedAt = DateTime.UtcNow;
    }

    public void Delete(string deletedBy)
    {
        if (IsDeleted) return;

        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        DeletedBy = deletedBy;
        Content = "[Đã xóa]";
    }

    public void AddAttachment(ChatAttachment attachment)
    {
        _attachments.Add(attachment);
    }
}

public class ChatAttachment : Entity
{
    public int ChatMessageId { get; private set; }
    public string FileName { get; private set; } = string.Empty;
    public string FileUrl { get; private set; } = string.Empty;
    public string? FileType { get; private set; }
    public long? FileSize { get; private set; }
    public DateTime UploadedAt { get; private set; }

    public virtual ChatMessage Message { get; private set; } = null!;

    private ChatAttachment() { }

    public static ChatAttachment Create(string fileName, string fileUrl, string? fileType = null, long? fileSize = null)
    {
        return new ChatAttachment
        {
            FileName = fileName,
            FileUrl = fileUrl,
            FileType = fileType,
            FileSize = fileSize,
            UploadedAt = DateTime.UtcNow
        };
    }
}
