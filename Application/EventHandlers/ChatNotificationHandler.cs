using Application.DTOs.Requests;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Enums;
using Domain.Events;

namespace Application.EventHandlers;

public class ChatNotificationHandler :
    IDomainEventHandler<ChatMessageSentEvent>,
    IDomainEventHandler<ChatRoomCreatedEvent>
{
    private readonly IChatRoomRepository _chatRoomRepository;
    private readonly INotificationService _notificationService;

    public ChatNotificationHandler(
        IChatRoomRepository chatRoomRepository,
        INotificationService notificationService)
    {
        _chatRoomRepository = chatRoomRepository;
        _notificationService = notificationService;
    }

    public async Task HandleAsync(ChatMessageSentEvent domainEvent, CancellationToken cancellationToken = default)
    {
        var room = await _chatRoomRepository.GetByIdWithParticipantsAsync(domainEvent.ChatRoomId);
        if (room == null) return;

        // Notify all other participants
        foreach (var participant in room.Participants.Where(p => p.UserId != domainEvent.SenderId && p.IsActive))
        {
            // Create notification
            await _notificationService.CreateNotificationAsync(new CreateNotificationRequest
            {
                UserId = participant.UserId,
                UserType = participant.UserType,
                Type = NotificationType.NewMessage,
                Title = "Tin nhắn mới",
                Message = domainEvent.Content.Length > 50 
                    ? domainEvent.Content[..50] + "..." 
                    : domainEvent.Content,
                ActionUrl = $"/chat/{domainEvent.ChatRoomId}",
                Icon = "comment",
                RelatedEntityId = domainEvent.ChatRoomId,
                RelatedEntityType = "ChatRoom"
            });
        }
    }

    public async Task HandleAsync(ChatRoomCreatedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        // Notify participants about new chat room
        if (domainEvent.Participant2Id.HasValue && domainEvent.Participant1Id.HasValue)
        {
            await _notificationService.CreateNotificationAsync(new CreateNotificationRequest
            {
                UserId = domainEvent.Participant2Id.Value,
                UserType = UserType.Customer, // Assuming participant 2 is customer
                Type = NotificationType.ChatRoomCreated,
                Title = "Cuộc trò chuyện mới",
                Message = "Bạn có cuộc trò chuyện mới. Hãy mở để xem chi tiết.",
                ActionUrl = $"/chat/{domainEvent.ChatRoomId}",
                Icon = "comments",
                RelatedEntityId = domainEvent.ChatRoomId,
                RelatedEntityType = "ChatRoom"
            });
        }
    }
}
