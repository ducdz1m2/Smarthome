using Application.DTOs.Requests;
using Application.DTOs.Responses;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities.Communication;
using Domain.Enums;
using Domain.Exceptions;
using Domain.Events;

namespace Application.Services;

public class ChatService : IChatService
{
    private readonly IChatRoomRepository _chatRoomRepository;
    private readonly IChatMessageRepository _chatMessageRepository;
    private readonly IDomainEventDispatcher _eventDispatcher;
    private readonly IInstallationService _installationService;

    public ChatService(
        IChatRoomRepository chatRoomRepository,
        IChatMessageRepository chatMessageRepository,
        IDomainEventDispatcher eventDispatcher,
        IInstallationService installationService)
    {
        _chatRoomRepository = chatRoomRepository;
        _chatMessageRepository = chatMessageRepository;
        _eventDispatcher = eventDispatcher;
        _installationService = installationService;
    }

    public async Task<List<ChatRoomResponse>> GetUserChatRoomsAsync(int userId, UserType userType)
    {
        var rooms = await _chatRoomRepository.GetByUserIdAsync(userId, userType);
        return rooms.Select(r => MapToRoomResponse(r, userId)).ToList();
    }

    public async Task<List<ChatRoomResponse>> GetAllSupportChatRoomsAsync()
    {
        var rooms = await _chatRoomRepository.GetActiveSupportRoomsAsync();
        return rooms.Select(r => MapToRoomResponse(r, 0)).ToList();
    }

    public async Task<ChatRoomResponse?> GetChatRoomByIdAsync(int id, int userId)
    {
        var room = await _chatRoomRepository.GetByIdWithParticipantsAsync(id);
        if (room == null) return null;
        return MapToRoomResponse(room, userId);
    }

    public async Task<List<ChatMessageResponse>> GetChatMessagesAsync(int chatRoomId, int userId, UserType userType, int limit = 50)
    {
        if (!await CanUserAccessChatRoomAsync(chatRoomId, userId, userType))
            throw new DomainException("Bạn không có quyền truy cập chat room này");

        var messages = await _chatMessageRepository.GetByChatRoomIdAsync(chatRoomId, limit);
        return messages.Select(m => MapToMessageResponse(m, userId)).ToList();
    }

    public async Task<int> GetUnreadMessageCountAsync(int userId, UserType userType)
    {
        var rooms = await _chatRoomRepository.GetByUserIdAsync(userId, userType);
        int total = 0;

        foreach (var room in rooms)
        {
            var participant = room.Participants.FirstOrDefault(p => p.UserId == userId && p.UserType == userType);
            if (participant != null)
                total += participant.UnreadCount;
        }

        return total;
    }

    public async Task<bool> CanUserAccessChatRoomAsync(int chatRoomId, int userId, UserType userType)
    {
        var room = await _chatRoomRepository.GetByIdWithParticipantsAsync(chatRoomId);
        if (room == null) return false;

        // Admin can access all Support chat rooms
        if (userType == UserType.Admin && room.Type == ChatRoomType.Support)
            return true;

        return room.Participants.Any(p => p.UserId == userId && p.UserType == userType && !p.IsBlocked);
    }

    public async Task<int> CreateOneToOneChatAsync(int user1Id, UserType user1Type, int user2Id, UserType user2Type, string? title = null)
    {
        // Check if room already exists
        var existingRoom = await _chatRoomRepository.GetOneToOneRoomAsync(user1Id, user1Type, user2Id, user2Type);
        if (existingRoom != null)
            return existingRoom.Id;

        var room = ChatRoom.CreateOneToOne(user1Id, user1Type, user2Id, user2Type, title);
        await _chatRoomRepository.AddAsync(room);
        await _chatRoomRepository.SaveChangesAsync();

        return room.Id;
    }

    public async Task<int> CreateSupportChatAsync(int customerId, int? orderId = null, int? installationId = null, int? warrantyClaimId = null)
    {
        var room = ChatRoom.CreateSupportRoom(customerId, orderId, installationId, warrantyClaimId);
        await _chatRoomRepository.AddAsync(room);
        await _chatRoomRepository.SaveChangesAsync();

        Console.WriteLine($"Created support chat room {room.Id} for customer {customerId}");

        // Assign admin to support chat room (using admin ID = 1 as default)
        // In production, this should get the first available admin or assign based on routing logic
        try
        {
            await AssignAdminAsync(room.Id, 1);
            Console.WriteLine($"Assigned admin ID 1 to chat room {room.Id}");
        }
        catch (Exception ex)
        {
            // Log but don't fail if admin assignment fails
            Console.WriteLine($"Failed to assign admin to support chat room: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }

        return room.Id;
    }

    public async Task<int> CreateInstallationChatAsync(int customerId, int technicianId, int installationId)
    {
        // Check if chat room already exists for this installation
        var existingRoom = await _chatRoomRepository.GetByInstallationIdAsync(installationId);
        if (existingRoom != null)
        {
            Console.WriteLine($"Installation chat room already exists: {existingRoom.Id}");
            return existingRoom.Id;
        }

        // Create installation chat room
        var room = ChatRoom.CreateInstallationRoom(customerId, installationId);
        await _chatRoomRepository.AddAsync(room);
        await _chatRoomRepository.SaveChangesAsync();

        Console.WriteLine($"Created installation chat room {room.Id} for installation {installationId}");

        // Assign technician to the chat room
        try
        {
            await AssignTechnicianAsync(room.Id, technicianId);
            Console.WriteLine($"Assigned technician ID {technicianId} to chat room {room.Id}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to assign technician to installation chat room: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }

        return room.Id;
    }

    public async Task<List<ChatRoomResponse>> GetInstallationChatRoomsAsync(int technicianId)
    {
        var rooms = await _chatRoomRepository.GetByTechnicianIdAsync(technicianId);
        return rooms.Select(r => MapToRoomResponse(r, technicianId)).ToList();
    }

    public async Task<List<ChatRoomResponse>> GetCustomerInstallationChatsAsync(int customerId)
    {
        var rooms = await _chatRoomRepository.GetByUserIdAsync(customerId, UserType.Customer);
        // Filter only installation chats (rooms with RelatedInstallationId)
        var installationRooms = rooms.Where(r => r.RelatedInstallationId.HasValue).ToList();
        return installationRooms.Select(r => MapToRoomResponse(r, customerId)).ToList();
    }

    public async Task CloseChatRoomAsync(int chatRoomId, int closedByUserId)
    {
        var room = await _chatRoomRepository.GetByIdAsync(chatRoomId);
        if (room == null)
            throw new DomainException("Không tìm thấy chat room");

        room.Close();
        _chatRoomRepository.Update(room);
        await _chatRoomRepository.SaveChangesAsync();
    }

    public async Task MarkChatAsReadAsync(int chatRoomId, int userId)
    {
        var room = await _chatRoomRepository.GetByIdWithParticipantsAsync(chatRoomId);
        if (room == null)
            throw new DomainException("Không tìm thấy chat room");

        room.MarkAsRead(userId);
        _chatRoomRepository.Update(room);
        await _chatRoomRepository.SaveChangesAsync();

        // Dispatch event
        await _eventDispatcher.DispatchAsync(new ChatMessageReadEvent(chatRoomId, userId, DateTime.UtcNow));
    }

    public async Task<int> SendMessageAsync(int chatRoomId, int senderId, UserType senderType, SendMessageRequest request)
    {
        var room = await _chatRoomRepository.GetByIdWithParticipantsAsync(chatRoomId);
        if (room == null)
            throw new DomainException("Không tìm thấy chat room");

        // Check permission
        if (!room.Participants.Any(p => p.UserId == senderId && p.UserType == senderType && !p.IsBlocked))
            throw new DomainException("Bạn không có quyền gửi tin nhắn trong chat room này");

        // Convert attachments
        List<ChatAttachment>? attachments = null;
        if (request.Attachments?.Any() == true)
        {
            attachments = request.Attachments.Select(a => ChatAttachment.Create(a.FileName, a.FileUrl, a.FileType, a.FileSize)).ToList();
        }

        room.AddMessage(senderId, senderType, request.Content, attachments);
        _chatRoomRepository.Update(room);
        await _chatRoomRepository.SaveChangesAsync();

        // Get the last added message
        var message = room.Messages.LastOrDefault();
        return message?.Id ?? 0;
    }

    public async Task EditMessageAsync(int messageId, int userId, EditMessageRequest request)
    {
        var message = await _chatMessageRepository.GetByIdAsync(messageId);
        if (message == null)
            throw new DomainException("Không tìm thấy tin nhắn");

        if (message.SenderId != userId)
            throw new DomainException("Bạn chỉ có thể chỉnh sửa tin nhắn của mình");

        message.Edit(request.Content);
        _chatMessageRepository.Update(message);
        await _chatMessageRepository.SaveChangesAsync();
    }

    public async Task DeleteMessageAsync(int messageId, int userId)
    {
        var message = await _chatMessageRepository.GetByIdAsync(messageId);
        if (message == null)
            throw new DomainException("Không tìm thấy tin nhắn");

        if (message.SenderId != userId)
            throw new DomainException("Bạn chỉ có thể xóa tin nhắn của mình");

        message.Delete(userId.ToString());
        _chatMessageRepository.Update(message);
        await _chatMessageRepository.SaveChangesAsync();
    }

    public async Task JoinSupportChatAsync(int chatRoomId, int userId, UserType userType)
    {
        var room = await _chatRoomRepository.GetByIdWithParticipantsAsync(chatRoomId);
        if (room == null)
            throw new DomainException("Không tìm thấy chat room");

        if (room.Type != ChatRoomType.Support)
            throw new DomainException("Chỉ có thể tham gia chat hỗ trợ");

        room.Join(userId, userType);
        _chatRoomRepository.Update(room);
        await _chatRoomRepository.SaveChangesAsync();
    }

    public async Task LeaveChatAsync(int chatRoomId, int userId)
    {
        var room = await _chatRoomRepository.GetByIdWithParticipantsAsync(chatRoomId);
        if (room == null)
            throw new DomainException("Không tìm thấy chat room");

        room.Leave(userId);
        _chatRoomRepository.Update(room);
        await _chatRoomRepository.SaveChangesAsync();
    }

    public async Task AssignTechnicianAsync(int chatRoomId, int technicianId)
    {
        var room = await _chatRoomRepository.GetByIdWithParticipantsAsync(chatRoomId);
        if (room == null)
            throw new DomainException("Không tìm thấy chat room");

        room.AssignTechnician(technicianId);
        _chatRoomRepository.Update(room);
        await _chatRoomRepository.SaveChangesAsync();
    }

    public async Task AssignAdminAsync(int chatRoomId, int adminId)
    {
        var room = await _chatRoomRepository.GetByIdWithParticipantsAsync(chatRoomId);
        if (room == null)
            throw new DomainException("Không tìm thấy chat room");

        room.AssignAdmin(adminId);
        _chatRoomRepository.Update(room);
        await _chatRoomRepository.SaveChangesAsync();
    }

    public async Task BlockParticipantAsync(int chatRoomId, int userId, string? reason = null)
    {
        var room = await _chatRoomRepository.GetByIdAsync(chatRoomId);
        if (room == null)
            throw new DomainException("Không tìm thấy chat room");

        room.BlockParticipant(userId, reason);
        _chatRoomRepository.Update(room);
        await _chatRoomRepository.SaveChangesAsync();
    }

    public async Task UnblockParticipantAsync(int chatRoomId, int userId)
    {
        var room = await _chatRoomRepository.GetByIdAsync(chatRoomId);
        if (room == null)
            throw new DomainException("Không tìm thấy chat room");

        room.UnblockParticipant(userId);
        _chatRoomRepository.Update(room);
        await _chatRoomRepository.SaveChangesAsync();
    }

    // Helper methods
    private async Task<ChatRoomResponse> MapToRoomResponseAsync(ChatRoom room, int currentUserId)
    {
        var lastMessage = room.Messages.OrderByDescending(m => m.SentAt).FirstOrDefault();

        var response = new ChatRoomResponse
        {
            Id = room.Id,
            Title = !string.IsNullOrEmpty(room.Title) ? room.Title : "Hỗ trợ khách hàng",
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
            LastMessage = lastMessage == null ? null : new ChatMessageSummaryResponse
            {
                Id = lastMessage.Id,
                Content = lastMessage.IsDeleted ? "[Đã xóa]" : lastMessage.Content,
                SenderId = lastMessage.SenderId,
                SenderType = lastMessage.SenderType.ToString(),
                SentAt = lastMessage.SentAt
            },
            UnreadCount = room.Participants.FirstOrDefault(p => p.UserId == currentUserId)?.UnreadCount ?? 0
        };

        // Load installation info if this is an installation chat
        if (room.RelatedInstallationId.HasValue)
        {
            try
            {
                var installation = await _installationService.GetByIdAsync(room.RelatedInstallationId.Value);
                if (installation != null)
                {
                    response.InstallationInfo = new InstallationInfoResponse
                    {
                        Id = installation.Id,
                        CustomerName = installation.CustomerName,
                        CustomerPhone = installation.CustomerPhone,
                        ShippingAddress = installation.ShippingAddress,
                        ScheduledDate = installation.ScheduledDate,
                        Status = installation.Status,
                        TechnicianName = installation.TechnicianName,
                        TechnicianPhone = installation.TechnicianPhone
                    };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load installation info: {ex.Message}");
            }
        }

        return response;
    }

    private static ChatRoomResponse MapToRoomResponse(ChatRoom room, int currentUserId)
    {
        var lastMessage = room.Messages.OrderByDescending(m => m.SentAt).FirstOrDefault();

        return new ChatRoomResponse
        {
            Id = room.Id,
            Title = !string.IsNullOrEmpty(room.Title) ? room.Title : "Hỗ trợ khách hàng",
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
            LastMessage = lastMessage == null ? null : new ChatMessageSummaryResponse
            {
                Id = lastMessage.Id,
                Content = lastMessage.IsDeleted ? "[Đã xóa]" : lastMessage.Content,
                SenderId = lastMessage.SenderId,
                SenderType = lastMessage.SenderType.ToString(),
                SentAt = lastMessage.SentAt
            },
            UnreadCount = room.Participants.FirstOrDefault(p => p.UserId == currentUserId)?.UnreadCount ?? 0
        };
    }

    private static ChatMessageResponse MapToMessageResponse(ChatMessage message, int currentUserId)
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
            IsFromMe = message.SenderId == currentUserId,
            Attachments = message.Attachments.Select(a => new ChatAttachmentResponse
            {
                Id = a.Id,
                FileName = a.FileName,
                FileUrl = a.FileUrl,
                FileType = a.FileType,
                FileSize = a.FileSize,
                FileSizeFormatted = FormatFileSize(a.FileSize)
            }).ToList()
        };
    }

    private static string FormatFileSize(long? bytes)
    {
        if (!bytes.HasValue) return "Unknown";
        if (bytes < 1024) return $"{bytes} B";
        if (bytes < 1024 * 1024) return $"{bytes / 1024:F1} KB";
        return $"{bytes / (1024 * 1024):F1} MB";
    }

    // This would normally come from authentication context
    private static UserType GetUserTypeFromContext()
    {
        // Placeholder - should be resolved from current user context
        return UserType.Customer;
    }
}
