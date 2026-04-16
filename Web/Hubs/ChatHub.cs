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

        public async Task SendMessage(int chatRoomId, int senderId, string senderType, string content)
        {
            Console.WriteLine($"[ChatHub] SendMessage called: chatRoomId={chatRoomId}, senderId={senderId}, senderType={senderType}, content={content}");
            
            var request = new Application.DTOs.Requests.SendMessageRequest
            {
                Content = content,
                Attachments = null
            };

            // Parse string to UserType enum
            if (!Enum.TryParse<UserType>(senderType, out var userType))
            {
                userType = UserType.Customer; // Default fallback
                Console.WriteLine($"[ChatHub] Failed to parse senderType '{senderType}', using default Customer");
            }

            try
            {
                var messageId = await _chatService.SendMessageAsync(chatRoomId, senderId, userType, request);
                Console.WriteLine($"[ChatHub] Message saved to DB with ID: {messageId}");

                await Clients.Group($"chat_{chatRoomId}").SendAsync("ReceiveMessage", new
                {
                    Id = messageId,
                    ChatRoomId = chatRoomId,
                    UserId = senderId,
                    SenderType = senderType.ToString(),
                    Content = content,
                    SentAt = DateTime.Now
                });
                Console.WriteLine($"[ChatHub] Message broadcasted to group chat_{chatRoomId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ChatHub] Error saving message: {ex.Message}");
                Console.WriteLine($"[ChatHub] Stack trace: {ex.StackTrace}");
                throw;
            }
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
