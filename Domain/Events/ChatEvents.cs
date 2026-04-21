namespace Domain.Events;

public record ChatMessageSentEvent(
    int ChatRoomId,
    int MessageId,
    int SenderId,
    int SenderType,
    string Content,
    DateTime SentAt) : DomainEvent(ChatRoomId, "ChatRoom");

public record ChatRoomCreatedEvent(
    int ChatRoomId,
    int? Participant1Id,
    int? Participant2Id,
    string RoomType) : DomainEvent(ChatRoomId, "ChatRoom");
