using Application.DTOs.Requests;
using Application.DTOs.Responses;
using Application.Interfaces.Services;
using Domain.Enums;
using Microsoft.AspNetCore.SignalR;

namespace Web.Hubs;

public class ChatHub : Hub
{
    private readonly IChatService _chatService;

    public ChatHub(IChatService chatService)
    {
        _chatService = chatService;
    }

    public async Task JoinChatRoom(int roomId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"chat_{roomId}");
    }

    public async Task LeaveChatRoom(int roomId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"chat_{roomId}");
    }

    public async Task SendMessage(int roomId, int senderId, string senderType, string content,
        string? fileUrl = null, string? fileName = null, string? fileType = null, long? fileSize = null)
    {
        if (!Enum.TryParse<UserType>(senderType, out var userType))
            userType = UserType.Customer;

        var request = new SendMessageRequest
        {
            Content = content,
            Attachments = fileUrl != null ? new List<ChatAttachmentRequest>
            {
                new ChatAttachmentRequest
                {
                    FileName = fileName ?? "file",
                    FileUrl = fileUrl,
                    FileType = fileType,
                    FileSize = fileSize
                }
            } : null
        };

        try
        {
            var message = await _chatService.SendMessageAsync(roomId, senderId, userType, request);
            await Clients.Group($"chat_{roomId}").SendAsync("ReceiveMessage", message);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ChatHub] Error sending message: {ex.Message}");
        }
    }
}
