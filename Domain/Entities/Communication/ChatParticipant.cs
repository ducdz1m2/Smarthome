using Domain.Abstractions;
using Domain.Enums;

namespace Domain.Entities.Communication;

public class ChatParticipant : Entity
{
    public int ChatRoomId { get; private set; }
    public int UserId { get; private set; }
    public UserType UserType { get; private set; }
    public DateTime JoinedAt { get; private set; }
    public DateTime? LeftAt { get; private set; }
    public bool IsActive { get; private set; } = true;
    public bool IsBlocked { get; private set; }
    public string? BlockReason { get; private set; }
    public int UnreadCount { get; private set; }
    public DateTime? LastReadAt { get; private set; }
    public DateTime? LastActivityAt { get; private set; }

    public virtual ChatRoom ChatRoom { get; private set; } = null!;

    private ChatParticipant() { }

    public static ChatParticipant Create(int chatRoomId, int userId, UserType userType)
    {
        return new ChatParticipant
        {
            ChatRoomId = chatRoomId,
            UserId = userId,
            UserType = userType,
            JoinedAt = DateTime.UtcNow,
            IsActive = true,
            IsBlocked = false,
            UnreadCount = 0
        };
    }

    public void Leave()
    {
        IsActive = false;
        LeftAt = DateTime.UtcNow;
    }

    public void Rejoin()
    {
        IsActive = true;
        LeftAt = null;
    }

    public void Block(string? reason = null)
    {
        IsBlocked = true;
        BlockReason = reason;
    }

    public void Unblock()
    {
        IsBlocked = false;
        BlockReason = null;
    }

    public void IncrementUnreadCount()
    {
        UnreadCount++;
    }

    public void MarkAsRead()
    {
        UnreadCount = 0;
        LastReadAt = DateTime.UtcNow;
    }

    public void UpdateLastActivity()
    {
        LastActivityAt = DateTime.UtcNow;
    }
}
