using Domain.Abstractions;
using Domain.Enums;

namespace Domain.Entities.Communication;

public enum ChatRoomType
{
    OneToOne = 0,
    Support = 1,
    Group = 2,
    Installation = 3
}

public class ChatRoom : Entity
{
    private readonly List<ChatMessage> _messages = new();
    private readonly List<ChatParticipant> _participants = new();

    public string Title { get; private set; } = string.Empty;
    public ChatRoomType Type { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime? ClosedAt { get; private set; }

    // Related entities
    public int? RelatedOrderId { get; private set; }
    public int? RelatedInstallationId { get; private set; }
    public int? RelatedWarrantyClaimId { get; private set; }

    public IReadOnlyCollection<ChatMessage> Messages => _messages.AsReadOnly();
    public IReadOnlyCollection<ChatParticipant> Participants => _participants.AsReadOnly();

    private ChatRoom() { }

    public static ChatRoom Create(
        string title,
        ChatRoomType type,
        string createdBy,
        int? relatedOrderId = null,
        int? relatedInstallationId = null,
        int? relatedWarrantyClaimId = null)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty");

        var room = new ChatRoom
        {
            Title = title.Trim(),
            Type = type,
            IsActive = true,
            RelatedOrderId = relatedOrderId,
            RelatedInstallationId = relatedInstallationId,
            RelatedWarrantyClaimId = relatedWarrantyClaimId,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };

        return room;
    }

    public void Close()
    {
        IsActive = false;
        ClosedAt = DateTime.UtcNow;
    }

    public void Reopen()
    {
        IsActive = true;
        ClosedAt = null;
    }

    public void AddMessage(ChatMessage message)
    {
        _messages.Add(message);
    }
}
