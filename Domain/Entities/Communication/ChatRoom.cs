using Domain.Abstractions;
using Domain.Enums;
using Domain.Events;

namespace Domain.Entities.Communication;

public class ChatRoom : AggregateRoot
{
    public string Title { get; private set; } = string.Empty;
    public ChatRoomType Type { get; private set; }
    public int? RelatedOrderId { get; private set; }
    public int? RelatedInstallationId { get; private set; }
    public int? RelatedWarrantyClaimId { get; private set; }
    public new DateTime CreatedAt { get; private set; }
    public DateTime? ClosedAt { get; private set; }
    public bool IsActive { get; private set; } = true;

    private readonly List<ChatParticipant> _participants = new();
    public IReadOnlyCollection<ChatParticipant> Participants => _participants.AsReadOnly();

    private readonly List<ChatMessage> _messages = new();
    public IReadOnlyCollection<ChatMessage> Messages => _messages.AsReadOnly();

    private ChatRoom() { }

    public static ChatRoom CreateOneToOne(
        int participant1Id, 
        UserType participant1Type,
        int participant2Id, 
        UserType participant2Type,
        string? title = null)
    {
        var room = new ChatRoom
        {
            Title = title ?? $"Chat {participant1Type}-{participant1Id} vs {participant2Type}-{participant2Id}",
            Type = ChatRoomType.OneToOne,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        room._participants.Add(ChatParticipant.Create(room.Id, participant1Id, participant1Type));
        room._participants.Add(ChatParticipant.Create(room.Id, participant2Id, participant2Type));

        room.AddDomainEvent(new ChatRoomCreatedEvent(room.Id, participant1Id, participant2Id));
        return room;
    }

    public static ChatRoom CreateSupportRoom(
        int customerId, 
        int? orderId = null, 
        int? installationId = null,
        int? warrantyClaimId = null)
    {
        var room = new ChatRoom
        {
            Title = "Hỗ trợ khách hàng",
            Type = ChatRoomType.Support,
            RelatedOrderId = orderId,
            RelatedInstallationId = installationId,
            RelatedWarrantyClaimId = warrantyClaimId,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        room._participants.Add(ChatParticipant.Create(room.Id, customerId, UserType.Customer));

        room.AddDomainEvent(new ChatRoomCreatedEvent(room.Id, customerId, null));
        return room;
    }

    public static ChatRoom CreateInstallationRoom(int customerId, int installationId)
    {
        var room = new ChatRoom
        {
            Title = $"Lắp đặt #{installationId}",
            Type = ChatRoomType.Support,
            RelatedInstallationId = installationId,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        room._participants.Add(ChatParticipant.Create(room.Id, customerId, UserType.Customer));

        room.AddDomainEvent(new ChatRoomCreatedEvent(room.Id, customerId, null));
        return room;
    }

    public void AddMessage(int senderId, UserType senderType, string content, List<ChatAttachment>? attachments = null)
    {
        if (!IsActive)
            throw new InvalidOperationException("Chat room is closed");

        if (!_participants.Any(p => p.UserId == senderId && p.UserType == senderType && !p.IsBlocked))
            throw new InvalidOperationException("User is not a participant or is blocked");

        var message = ChatMessage.Create(Id, senderId, senderType, content, attachments);
        _messages.Add(message);

        // Update last activity
        var participant = _participants.First(p => p.UserId == senderId);
        participant.UpdateLastActivity();

        // Reset unread count for sender, increment for others
        foreach (var p in _participants.Where(p => p.UserId != senderId))
        {
            p.IncrementUnreadCount();
        }

        AddDomainEvent(new ChatMessageSentEvent(Id, senderId, message.Id, content));
    }

    public void MarkAsRead(int userId)
    {
        var participant = _participants.FirstOrDefault(p => p.UserId == userId);
        participant?.MarkAsRead();
    }

    public void Join(int userId, UserType userType)
    {
        if (_participants.Any(p => p.UserId == userId))
            return;

        _participants.Add(ChatParticipant.Create(Id, userId, userType));
    }

    public void Leave(int userId)
    {
        var participant = _participants.FirstOrDefault(p => p.UserId == userId);
        if (participant != null)
        {
            participant.Leave();
        }
    }

    public void AssignTechnician(int technicianId)
    {
        if (Type != ChatRoomType.Support)
            throw new InvalidOperationException("Only support rooms can be assigned");

        var existingTech = _participants.FirstOrDefault(p => p.UserType == UserType.Technician);
        if (existingTech != null)
        {
            existingTech.Leave();
        }

        _participants.Add(ChatParticipant.Create(Id, technicianId, UserType.Technician));
        
        AddDomainEvent(new TechnicianAssignedToChatEvent(Id, technicianId));
    }

    public void AssignAdmin(int adminId)
    {
        if (_participants.Any(p => p.UserType == UserType.Admin))
            return;

        _participants.Add(ChatParticipant.Create(Id, adminId, UserType.Admin));
    }

    public void Close()
    {
        IsActive = false;
        ClosedAt = DateTime.UtcNow;
        AddDomainEvent(new ChatRoomClosedEvent(Id));
    }

    public void BlockParticipant(int userId, string? reason = null)
    {
        var participant = _participants.FirstOrDefault(p => p.UserId == userId);
        participant?.Block(reason);
    }

    public void UnblockParticipant(int userId)
    {
        var participant = _participants.FirstOrDefault(p => p.UserId == userId);
        participant?.Unblock();
    }
}
