using Application.DTOs.Requests;
using Application.DTOs.Responses;
using Domain.Enums;

namespace Application.Interfaces.Services;

public interface IChatService
{
    // Query methods
    Task<List<ChatRoomResponse>> GetUserChatRoomsAsync(int userId, UserType userType);
    Task<List<ChatRoomResponse>> GetAllSupportChatRoomsAsync();
    Task<ChatRoomResponse?> GetChatRoomByIdAsync(int id, int userId);
    Task<List<ChatMessageResponse>> GetChatMessagesAsync(int chatRoomId, int userId, int limit = 50);
    Task<int> GetUnreadMessageCountAsync(int userId, UserType userType);
    Task<bool> CanUserAccessChatRoomAsync(int chatRoomId, int userId, UserType userType);

    // Chat Room management
    Task<int> CreateOneToOneChatAsync(int user1Id, UserType user1Type, int user2Id, UserType user2Type, string? title = null);
    Task<int> CreateSupportChatAsync(int customerId, int? orderId = null, int? installationId = null, int? warrantyClaimId = null);
    Task CloseChatRoomAsync(int chatRoomId, int closedByUserId);
    Task MarkChatAsReadAsync(int chatRoomId, int userId);

    // Messaging
    Task<int> SendMessageAsync(int chatRoomId, int senderId, UserType senderType, SendMessageRequest request);
    Task EditMessageAsync(int messageId, int userId, EditMessageRequest request);
    Task DeleteMessageAsync(int messageId, int userId);

    // Participant management (Admin/Technician)
    Task JoinSupportChatAsync(int chatRoomId, int userId, UserType userType);
    Task LeaveChatAsync(int chatRoomId, int userId);
    Task AssignTechnicianAsync(int chatRoomId, int technicianId);
    Task AssignAdminAsync(int chatRoomId, int adminId);

    // Moderation
    Task BlockParticipantAsync(int chatRoomId, int userId, string? reason = null);
    Task UnblockParticipantAsync(int chatRoomId, int userId);
}
