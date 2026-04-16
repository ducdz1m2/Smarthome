using Application.Interfaces.Services;
using Domain.Enums;
using Microsoft.AspNetCore.SignalR;

namespace Web.Hubs
{
    public class ChatHub : Hub
    {
        private readonly IChatService _chatService;

        public ChatHub(IChatService chatService)
        {
            _chatService = chatService;
        }

        public async Task SendMessage(int chatRoomId, int senderId, UserType senderType, string content)
        {
            var request = new Application.DTOs.Requests.SendMessageRequest
            {
                Content = content,
                Attachments = null
            };

            var messageId = await _chatService.SendMessageAsync(chatRoomId, senderId, senderType, request);

            await Clients.Group($"chat_{chatRoomId}").SendAsync("ReceiveMessage", new
            {
                Id = messageId,
                ChatRoomId = chatRoomId,
                SenderId = senderId,
                SenderType = senderType.ToString(),
                Content = content,
                SentAt = DateTime.Now
            });
        }

        public async Task JoinChatRoom(int chatRoomId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"chat_{chatRoomId}");
        }

        public async Task JoinAdminGroup()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "admins");
        }

        public async Task MarkAsRead(int chatRoomId, int userId)
        {
            await _chatService.MarkChatAsReadAsync(chatRoomId, userId);
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await base.OnDisconnectedAsync(exception);
        }
    }
}
