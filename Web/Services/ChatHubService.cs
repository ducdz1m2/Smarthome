using Application.DTOs.Responses;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.SignalR;
using Web.Hubs;

namespace Web.Services;

public class ChatHubService : IChatHubService
{
    private readonly IHubContext<ChatHub> _hubContext;

    public ChatHubService(IHubContext<ChatHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task NotifyNewMessageAsync(int roomId, ChatMessageResponse message)
    {
        await _hubContext.Clients.Group($"chat_{roomId}").SendAsync("ReceiveMessage", message);
    }

    public async Task NotifyChatRoomCreatedAsync(int roomId, int userId, string userType)
    {
        // For now, notify the specific user or broad group if needed
        // Typically, we might notify a "lobby" or "admin" group
        if (userType == "Admin")
        {
            await _hubContext.Clients.All.SendAsync("ChatRoomCreated", roomId);
        }
    }
}
