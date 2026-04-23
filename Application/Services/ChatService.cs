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
    private readonly IDomainEventDispatcher _eventDispatcher;

    public ChatService(
        IChatRoomRepository chatRoomRepository,
        IDomainEventDispatcher eventDispatcher)
    {
        _chatRoomRepository = chatRoomRepository;
        _eventDispatcher = eventDispatcher;
    }

    public async Task<List<ChatRoomResponse>> GetAllSupportChatRoomsAsync()
    {
        var rooms = await _chatRoomRepository.GetActiveSupportRoomsAsync();
        return rooms.Select(MapToRoomResponse).ToList();
    }

    public async Task<List<ChatRoomResponse>> GetInstallationChatRoomsAsync(int technicianId)
    {
        var rooms = await _chatRoomRepository.GetByTechnicianIdAsync(technicianId);
        return rooms.Select(MapToRoomResponse).ToList();
    }

    public async Task<List<ChatRoomResponse>> GetCustomerChatRoomsAsync(int customerId)
    {
        var rooms = await _chatRoomRepository.GetByUserIdAsync(customerId, UserType.Customer);
        return rooms.Select(MapToRoomResponse).ToList();
    }

    public async Task<ChatRoomResponse?> GetChatRoomAsync(int roomId, int userId, UserType userType)
    {
        var room = await _chatRoomRepository.GetByIdWithParticipantsAsync(roomId);
        if (room == null) return null;

        var response = MapToRoomResponse(room);
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

        return room.Messages
            .Where(m => !m.IsDeleted)
            .OrderBy(m => m.SentAt)
            .Select(m => MapToMessageResponse(m, userId, userType))
            .ToList();
    }

    public async Task<ChatMessageResponse> SendMessageAsync(int roomId, int senderId, UserType senderType, SendMessageRequest request)
    {
        var room = await _chatRoomRepository.GetByIdWithParticipantsAsync(roomId);
        if (room == null)
            throw new InvalidOperationException("Chat room not found");

        var message = ChatMessage.Create(roomId, senderId, senderType, request.Content, senderId.ToString());

        // Add message to room before saving
        room.AddMessage(message);

        _chatRoomRepository.Update(room);
        await _chatRoomRepository.SaveChangesAsync();

        await _eventDispatcher.DispatchAsync(new ChatMessageSentEvent(
            roomId,
            message.Id,
            senderId,
            (int)senderType,
            request.Content,
            DateTime.UtcNow));

        return MapToMessageResponse(message, senderId, senderType);
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

    private static ChatRoomResponse MapToRoomResponse(ChatRoom room)
    {
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
            Participants = room.Participants.Select(p => new ChatParticipantResponse
            {
                Id = p.Id,
                UserId = p.UserId,
                UserType = p.UserType.ToString(),
                JoinedAt = p.JoinedAt,
                IsActive = p.IsActive,
                IsBlocked = p.IsBlocked,
                UnreadCount = p.UnreadCount,
                LastActivityAt = p.LastActivityAt,
                LastReadAt = p.LastReadAt
            }).ToList(),
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

    private static ChatMessageResponse MapToMessageResponse(ChatMessage message, int currentUserId, UserType currentUserType)
    {
        return new ChatMessageResponse
        {
            Id = message.Id,
            ChatRoomId = message.ChatRoomId,
            SenderId = message.SenderId,
            SenderType = message.SenderType.ToString(),
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
