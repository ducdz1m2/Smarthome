using Domain.Abstractions;
using Domain.Enums;

namespace Domain.Entities.Communication;

public class ChatMessage : Entity
{
    private readonly List<ChatAttachment> _attachments = new();

    public int ChatRoomId { get; private set; }
    public ChatRoom ChatRoom { get; private set; } = null!;

    public int SenderId { get; private set; }
    public UserType SenderType { get; private set; }

    public string Content { get; private set; } = string.Empty;
    public DateTime SentAt { get; private set; }
    public DateTime? EditedAt { get; private set; }

    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }
    public string? DeletedBy { get; private set; }

    public IReadOnlyCollection<ChatAttachment> Attachments => _attachments.AsReadOnly();

    private ChatMessage() { }

    public static ChatMessage Create(int chatRoomId, int senderId, UserType senderType, string content, string createdBy)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Content cannot be empty");

        return new ChatMessage
        {
            ChatRoomId = chatRoomId,
            SenderId = senderId,
            SenderType = senderType,
            Content = content.Trim(),
            SentAt = DateTime.UtcNow,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };
    }

    public void Edit(string newContent)
    {
        if (string.IsNullOrWhiteSpace(newContent))
            throw new ArgumentException("Content cannot be empty");

        Content = newContent.Trim();
        EditedAt = DateTime.UtcNow;
    }

    public void Delete(string deletedBy)
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        DeletedBy = deletedBy;
    }
}
