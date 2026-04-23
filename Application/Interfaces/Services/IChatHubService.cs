using Application.DTOs.Responses;

namespace Application.Interfaces.Services;

public interface IChatHubService
{
    Task NotifyNewMessageAsync(int roomId, ChatMessageResponse message);
    Task NotifyChatRoomCreatedAsync(int roomId, int userId, string userType);
}
