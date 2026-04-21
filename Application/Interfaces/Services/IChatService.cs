using Application.DTOs.Requests;
using Application.DTOs.Responses;
using Domain.Enums;

namespace Application.Interfaces.Services;

public interface IChatService
{
    // Room management
    Task<List<ChatRoomResponse>> GetAllSupportChatRoomsAsync();
    Task<List<ChatRoomResponse>> GetInstallationChatRoomsAsync(int technicianId);
    Task<List<ChatRoomResponse>> GetCustomerChatRoomsAsync(int customerId);
    Task<ChatRoomResponse?> GetChatRoomAsync(int roomId, int userId, UserType userType);
    Task<int> CreateInstallationChatAsync(int customerId, int technicianId, int installationId);
    Task<int> CreateSupportChatAsync(CreateSupportChatRequest request, int adminId);

    // Messages
    Task<List<ChatMessageResponse>> GetChatMessagesAsync(int roomId, int userId, UserType userType);
    Task<ChatMessageResponse> SendMessageAsync(int roomId, int senderId, UserType senderType, SendMessageRequest request);
    Task<ChatMessageResponse?> EditMessageAsync(int messageId, int userId, EditMessageRequest request);
    Task<bool> DeleteMessageAsync(int messageId, int userId);

    // Participants
    Task MarkChatAsReadAsync(int roomId, int userId);
    Task<bool> CloseChatRoomAsync(int roomId, int userId);
}
