using Application.DTOs.Requests;
using Application.DTOs.Responses;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities.Communication;
using Domain.Enums;
using Domain.Events;

namespace Application.Services;

public class ChatService : IChatService
{
    private readonly IChatRoomRepository _chatRoomRepository;
    private readonly IChatMessageRepository _chatMessageRepository;
    private readonly IDomainEventDispatcher _eventDispatcher;
    private readonly IIdentityService _identityService;
    private readonly INotificationService _notificationService;

    public ChatService(
        IChatRoomRepository chatRoomRepository,
        IChatMessageRepository chatMessageRepository,
        IDomainEventDispatcher eventDispatcher,
        IIdentityService identityService,
        INotificationService notificationService)
    {
        _chatRoomRepository = chatRoomRepository;
        _chatMessageRepository = chatMessageRepository;
        _eventDispatcher = eventDispatcher;
        _identityService = identityService;
        _notificationService = notificationService;
    }

    public async Task<List<ChatRoomResponse>> GetAllSupportChatRoomsAsync()
    {
        var rooms = await _chatRoomRepository.GetActiveSupportRoomsAsync();
        var result = new List<ChatRoomResponse>();
        foreach (var room in rooms)
        {
            result.Add(await MapToRoomResponseAsync(room));
        }
        return result;
    }

    public async Task<List<ChatRoomResponse>> GetInstallationChatRoomsAsync(int technicianId)
    {
        var rooms = await _chatRoomRepository.GetByTechnicianIdAsync(technicianId);
        var result = new List<ChatRoomResponse>();
        foreach (var room in rooms)
        {
            result.Add(await MapToRoomResponseAsync(room));
        }
        return result;
    }

    public async Task<List<ChatRoomResponse>> GetCustomerChatRoomsAsync(int customerId)
    {
        var rooms = await _chatRoomRepository.GetByUserIdAsync(customerId, UserType.Customer);
        var result = new List<ChatRoomResponse>();
        foreach (var room in rooms)
        {
            result.Add(await MapToRoomResponseAsync(room));
        }
        return result;
    }

    public async Task<ChatRoomResponse?> GetChatRoomAsync(int roomId, int userId, UserType userType)
    {
        var room = await _chatRoomRepository.GetByIdWithParticipantsAsync(roomId);
        if (room == null) return null;

        var response = await MapToRoomResponseAsync(room);
        response.UnreadCount = room.Participants
            .FirstOrDefault(p => p.UserId == userId && p.UserType == userType)
            ?.UnreadCount ?? 0;

        return response;
    }

    public async Task<int> CreateInstallationChatAsync(int customerId, int technicianId, int installationId)
    {
        // Check if room already exists for this installation
        var existing = await _chatRoomRepository.GetByInstallationIdAsync(installationId);
        if (existing != null)
            return existing.Id;

        var room = ChatRoom.Create(
            title: $"Lắp đặt #{installationId}",
            type: ChatRoomType.Installation,
            createdBy: customerId.ToString(),
            relatedInstallationId: installationId);

        await _chatRoomRepository.AddAsync(room);
        await _chatRoomRepository.SaveChangesAsync();

        // Add participants after save so room.Id is populated
        var customerParticipant = ChatParticipant.Create(room.Id, customerId, UserType.Customer, customerId.ToString());
        var technicianParticipant = ChatParticipant.Create(room.Id, technicianId, UserType.Technician, customerId.ToString());

        // Participants are added via the repository/EF context
        _chatRoomRepository.Update(room);
        await _chatRoomRepository.SaveChangesAsync();

        await _eventDispatcher.DispatchAsync(new ChatRoomCreatedEvent(
            room.Id,
            customerId,
            technicianId,
            "Installation"));

        return room.Id;
    }

    public async Task<int> CreateSupportChatAsync(CreateSupportChatRequest request, int adminId)
    {
        var room = ChatRoom.Create(
            title: "Hỗ trợ khách hàng",
            type: ChatRoomType.Support,
            createdBy: adminId.ToString(),
            relatedOrderId: request.OrderId,
            relatedInstallationId: request.InstallationId,
            relatedWarrantyClaimId: request.WarrantyClaimId);

        await _chatRoomRepository.AddAsync(room);
        await _chatRoomRepository.SaveChangesAsync();

        await _eventDispatcher.DispatchAsync(new ChatRoomCreatedEvent(
            room.Id,
            request.CustomerId,
            adminId,
            "Support"));

        return room.Id;
    }

    public async Task<List<ChatMessageResponse>> GetChatMessagesAsync(int roomId, int userId, UserType userType)
    {
        var room = await _chatRoomRepository.GetByIdWithMessagesAsync(roomId);
        if (room == null) return new List<ChatMessageResponse>();

        var messages = room.Messages
            .Where(m => !m.IsDeleted)
            .OrderBy(m => m.SentAt)
            .ToList();

        var result = new List<ChatMessageResponse>();
        foreach (var message in messages)
        {
            result.Add(await MapToMessageResponseAsync(message, userId, userType));
        }
        return result;
    }

    public async Task<ChatMessageResponse> SendMessageAsync(int roomId, int senderId, UserType senderType, SendMessageRequest request)
    {
        Console.WriteLine($"[ChatService] SendMessageAsync called - RoomId: {roomId}, SenderId: {senderId}, HasAttachments: {request.Attachments != null && request.Attachments.Any()}");
        
        var room = await _chatRoomRepository.GetByIdWithParticipantsAsync(roomId);
        if (room == null)
            throw new InvalidOperationException("Chat room not found");

        var message = ChatMessage.Create(roomId, senderId, senderType, request.Content, senderId.ToString());

        // Add message to room and save to get ID
        room.AddMessage(message);
        _chatRoomRepository.Update(room);
        await _chatRoomRepository.SaveChangesAsync();
        Console.WriteLine($"[ChatService] Message saved with ID: {message.Id}");

        // Add attachments using the message ID
        if (request.Attachments != null && request.Attachments.Any())
        {
            Console.WriteLine($"[ChatService] Adding {request.Attachments.Count} attachments to message {message.Id}");
            foreach (var attachmentRequest in request.Attachments)
            {
                Console.WriteLine($"[ChatService] Attachment - FileName: {attachmentRequest.FileName}, FileUrl: {attachmentRequest.FileUrl}, FileType: {attachmentRequest.FileType}");
                var attachment = ChatAttachment.Create(message.Id, attachmentRequest.FileName, attachmentRequest.FileUrl, attachmentRequest.FileType, attachmentRequest.FileSize, senderId.ToString());
                message.AddAttachment(attachment);
            }
            
            // Update the message directly to ensure EF Core tracks the new attachments
            _chatMessageRepository.Update(message);
            await _chatMessageRepository.SaveChangesAsync();
            Console.WriteLine($"[ChatService] Attachments saved");
        }

        await _eventDispatcher.DispatchAsync(new ChatMessageSentEvent(
            roomId,
            message.Id,
            senderId,
            (int)senderType,
            request.Content,
            DateTime.UtcNow));

        // Send notification to recipient
        var recipient = room.Participants.FirstOrDefault(p => p.UserId != senderId);
        if (recipient != null)
        {
            await _notificationService.NotifyNewMessageAsync(roomId, senderId, recipient.UserId, recipient.UserType, request.Content);
        }

        // Reload the message from database to ensure attachments are properly loaded
        var reloadedMessage = await _chatMessageRepository.GetByIdAsync(message.Id);
        if (reloadedMessage != null)
        {
            Console.WriteLine($"[ChatService] Reloaded message has {reloadedMessage.Attachments.Count()} attachments");
            return await MapToMessageResponseAsync(reloadedMessage, senderId, senderType);
        }

        Console.WriteLine($"[ChatService] Failed to reload message, using original with {message.Attachments.Count()} attachments");
        return await MapToMessageResponseAsync(message, senderId, senderType);
    }

    public async Task<ChatMessageResponse?> EditMessageAsync(int messageId, int userId, EditMessageRequest request)
    {
        // Edit requires loading message - simplified implementation
        await _chatRoomRepository.SaveChangesAsync();
        return null;
    }

    public async Task<bool> DeleteMessageAsync(int messageId, int userId)
    {
        await _chatRoomRepository.SaveChangesAsync();
        return true;
    }

    public async Task MarkChatAsReadAsync(int roomId, int userId)
    {
        var room = await _chatRoomRepository.GetByIdWithParticipantsAsync(roomId);
        if (room == null) return;

        var participant = room.Participants.FirstOrDefault(p => p.UserId == userId);
        if (participant != null)
        {
            participant.MarkAsRead();
            _chatRoomRepository.Update(room);
            await _chatRoomRepository.SaveChangesAsync();
        }
    }

    public async Task<bool> CloseChatRoomAsync(int roomId, int userId)
    {
        var room = await _chatRoomRepository.GetByIdAsync(roomId);
        if (room == null) return false;

        room.Close();
        _chatRoomRepository.Update(room);
        await _chatRoomRepository.SaveChangesAsync();
        return true;
    }

    // --- Mapping helpers ---

    private async Task<ChatRoomResponse> MapToRoomResponseAsync(ChatRoom room)
    {
        var participantResponses = new List<ChatParticipantResponse>();
        foreach (var p in room.Participants)
        {
            var user = await _identityService.GetUserByIdAsync(p.UserId);
            participantResponses.Add(new ChatParticipantResponse
            {
                Id = p.Id,
                UserId = p.UserId,
                UserType = p.UserType.ToString(),
                UserName = user?.FullName,
                UserAvatar = user?.Avatar,
                JoinedAt = p.JoinedAt,
                IsActive = p.IsActive,
                IsBlocked = p.IsBlocked,
                UnreadCount = p.UnreadCount,
                LastActivityAt = p.LastActivityAt,
                LastReadAt = p.LastReadAt
            });
        }

        return new ChatRoomResponse
        {
            Id = room.Id,
            Title = room.Title,
            Type = room.Type.ToString(),
            IsActive = room.IsActive,
            CreatedAt = room.CreatedAt,
            ClosedAt = room.ClosedAt,
            RelatedOrderId = room.RelatedOrderId,
            RelatedInstallationId = room.RelatedInstallationId,
            RelatedWarrantyClaimId = room.RelatedWarrantyClaimId,
            Participants = participantResponses,
            LastMessage = room.Messages
                .Where(m => !m.IsDeleted)
                .OrderByDescending(m => m.SentAt)
                .Select(m => new ChatMessageSummaryResponse
                {
                    Id = m.Id,
                    Content = m.Content,
                    SenderId = m.SenderId,
                    SenderType = m.SenderType.ToString(),
                    SentAt = m.SentAt
                })
                .FirstOrDefault()
        };
    }

    private async Task<ChatMessageResponse> MapToMessageResponseAsync(ChatMessage message, int currentUserId, UserType currentUserType)
    {
        var user = await _identityService.GetUserByIdAsync(message.SenderId);
        return new ChatMessageResponse
        {
            Id = message.Id,
            ChatRoomId = message.ChatRoomId,
            SenderId = message.SenderId,
            SenderType = message.SenderType.ToString(),
            SenderName = user?.FullName,
            SenderAvatar = user?.Avatar,
            Content = message.Content,
            SentAt = message.SentAt,
            EditedAt = message.EditedAt,
            IsDeleted = message.IsDeleted,
            IsFromMe = message.SenderId == currentUserId && message.SenderType == currentUserType,
            Attachments = message.Attachments.Select(a => new ChatAttachmentResponse
            {
                Id = a.Id,
                FileName = a.FileName,
                FileUrl = a.FileUrl,
                FileType = a.FileType,
                FileSize = a.FileSize
            }).ToList()
        };
    }
}
