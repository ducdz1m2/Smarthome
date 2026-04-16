using Microsoft.AspNetCore.SignalR;

namespace Web.Hubs
{
    public class NotificationHub : Hub
    {
        public async Task JoinUserGroup(string userId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
        }

        public async Task JoinPublicGroup()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "all");
        }

        public async Task JoinAdminNotifGroup()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "admin_notif");
        }
    }
}
