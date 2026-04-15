namespace Domain.Events;

// Chat Room Events
public record ChatRoomCreatedEvent(int ChatRoomId, int? Participant1Id, int? Participant2Id) : DomainEvent;

public record ChatRoomClosedEvent(int ChatRoomId) : DomainEvent;

public record TechnicianAssignedToChatEvent(int ChatRoomId, int TechnicianId) : DomainEvent;

// Chat Message Events
public record ChatMessageSentEvent(int ChatRoomId, int SenderId, int MessageId, string Content) : DomainEvent;

public record ChatMessageReadEvent(int ChatRoomId, int UserId, DateTime ReadAt) : DomainEvent;

// Participant Events
public record ParticipantJoinedEvent(int ChatRoomId, int UserId, string UserType) : DomainEvent;

public record ParticipantLeftEvent(int ChatRoomId, int UserId) : DomainEvent;
