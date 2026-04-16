using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.Components;

namespace Web.Services;

public class SignalRService
{
    private HubConnection? _chatConnection;
    private HubConnection? _notificationConnection;
    private HubConnection? _installationConnection;
    private readonly NavigationManager _navigationManager;

    public event Action<object>? OnChatMessageReceived;
    public event Action<object>? OnNotificationReceived;
    public event Action<string>? OnInstallationUpdate;

    public SignalRService(NavigationManager navigationManager)
    {
        _navigationManager = navigationManager;
    }

    public async Task InitializeAsync()
    {
        var baseUrl = _navigationManager.BaseUri;
        Console.WriteLine($"Initializing SignalR with base URL: {baseUrl}");

        _chatConnection = new HubConnectionBuilder()
            .WithUrl($"{baseUrl}hubs/chat")
            .WithAutomaticReconnect()
            .Build();

        _notificationConnection = new HubConnectionBuilder()
            .WithUrl($"{baseUrl}hubs/notification")
            .WithAutomaticReconnect()
            .Build();

        _installationConnection = new HubConnectionBuilder()
            .WithUrl($"{baseUrl}hubs/installation")
            .WithAutomaticReconnect()
            .Build();

        _chatConnection.On<object>("ReceiveMessage", (message) =>
        {
            OnChatMessageReceived?.Invoke(message);
        });

        _notificationConnection.On<object>("ReceiveNotification", (notification) =>
        {
            OnNotificationReceived?.Invoke(notification);
        });

        _installationConnection.On<string>("BookingCancelled", (message) =>
        {
            OnInstallationUpdate?.Invoke($"BookingCancelled: {message}");
        });

        _installationConnection.On<string>("JobCompleted", (message) =>
        {
            OnInstallationUpdate?.Invoke($"JobCompleted: {message}");
        });

        await Task.WhenAll(
            _chatConnection.StartAsync(),
            _notificationConnection.StartAsync(),
            _installationConnection.StartAsync()
        );
    }

    public async Task JoinChatRoom(int chatRoomId)
    {
        if (_chatConnection != null)
        {
            await _chatConnection.InvokeAsync("JoinChatRoom", chatRoomId);
        }
    }

    public async Task SendMessage(int chatRoomId, int senderId, string senderType, string content)
    {
        if (_chatConnection != null)
        {
            await _chatConnection.InvokeAsync("SendMessage", chatRoomId, senderId, senderType, content);
        }
    }

    public async Task JoinUserNotificationGroup(string userId)
    {
        if (_notificationConnection != null)
        {
            await _notificationConnection.InvokeAsync("JoinUserGroup", userId);
        }
    }

    public async Task JoinAdminNotificationGroup()
    {
        if (_notificationConnection != null)
        {
            await _notificationConnection.InvokeAsync("JoinAdminNotifGroup");
        }
    }

    public async Task JoinTechnicianGroup(int technicianId)
    {
        if (_installationConnection != null)
        {
            await _installationConnection.InvokeAsync("JoinTechnicianGroup", technicianId);
        }
    }

    public async Task JoinAdminInstallationGroup()
    {
        if (_installationConnection != null)
        {
            await _installationConnection.InvokeAsync("JoinAdminInstallationGroup");
        }
    }
}
