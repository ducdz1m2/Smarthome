using Domain.Abstractions;
using Domain.Enums;

namespace Domain.Entities.Communication;

public class ChatParticipant : Entity
{
    public int ChatRoomId { get; private set; }
    public ChatRoom ChatRoom { get; private set; } = null!;

    public int UserId { get; private set; }
    public UserType UserType { get; private set; }

    public bool IsActive { get; private set; }
    public bool IsBlocked { get; private set; }
    public string? BlockReason { get; private set; }

    public DateTime JoinedAt { get; private set; }
    public DateTime? LeftAt { get; private set; }
    public DateTime? LastActivityAt { get; private set; }
    public DateTime? LastReadAt { get; private set; }
    public int UnreadCount { get; private set; }

    private ChatParticipant() { }

    public static ChatParticipant Create(int chatRoomId, int userId, UserType userType, string createdBy)
    {
        return new ChatParticipant
        {
            ChatRoomId = chatRoomId,
            UserId = userId,
            UserType = userType,
            IsActive = true,
            IsBlocked = false,
            JoinedAt = DateTime.UtcNow,
            UnreadCount = 0,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };
    }

    public void Leave()
    {
        IsActive = false;
        LeftAt = DateTime.UtcNow;
    }

    public void Block(string reason)
    {
        IsBlocked = true;
        BlockReason = reason;
    }

    public void Unblock()
    {
        IsBlocked = false;
        BlockReason = null;
    }

    public void UpdateLastActivity()
    {
        LastActivityAt = DateTime.UtcNow;
    }

    public void MarkAsRead()
    {
        LastReadAt = DateTime.UtcNow;
        UnreadCount = 0;
    }

    public void IncrementUnread()
    {
        UnreadCount++;
    }
}
