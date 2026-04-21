using Application.DTOs.Responses;
using Microsoft.AspNetCore.SignalR.Client;

namespace Web.Services;

public class SignalRService : IAsyncDisposable
{
    private HubConnection? _notificationHub;
    private HubConnection? _chatHub;
    private readonly IConfiguration _configuration;
    private readonly LocalAuthStateProvider _authStateProvider;
    private bool _initialized = false;

    public event Action<object>? OnNotificationReceived;
    public event Action<ChatMessageResponse>? OnChatMessageReceived;

    public SignalRService(IConfiguration configuration, LocalAuthStateProvider authStateProvider)
    {
        _configuration = configuration;
        _authStateProvider = authStateProvider;
    }

    public async Task InitializeAsync()
    {
        if (_initialized) return;

        var baseUrl = "https://localhost:7298";

        // Notification hub
        _notificationHub = new HubConnectionBuilder()
            .WithUrl($"{baseUrl}/hubs/notification", options =>
            {
                options.AccessTokenProvider = async () => await _authStateProvider.GetTokenAsync();
            })
            .WithAutomaticReconnect()
            .Build();

        _notificationHub.On<object>("ReceiveNotification", (notification) =>
        {
            OnNotificationReceived?.Invoke(notification);
        });

        // Chat hub
        _chatHub = new HubConnectionBuilder()
            .WithUrl($"{baseUrl}/hubs/chat", options =>
            {
                options.AccessTokenProvider = async () => await _authStateProvider.GetTokenAsync();
            })
            .WithAutomaticReconnect()
            .Build();

        _chatHub.On<ChatMessageResponse>("ReceiveMessage", (message) =>
        {
            OnChatMessageReceived?.Invoke(message);
        });

        try
        {
            await _notificationHub.StartAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SignalRService] Failed to connect notification hub: {ex.Message}");
        }

        try
        {
            await _chatHub.StartAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SignalRService] Failed to connect chat hub: {ex.Message}");
        }

        _initialized = true;
    }

    public async Task JoinUserNotificationGroup(string userId)
    {
        if (_notificationHub?.State == HubConnectionState.Connected)
            await _notificationHub.InvokeAsync("JoinUserGroup", userId);
    }

    public async Task JoinAdminNotificationGroup()
    {
        if (_notificationHub?.State == HubConnectionState.Connected)
            await _notificationHub.InvokeAsync("JoinAdminNotifGroup");
    }

    public async Task JoinChatRoom(int roomId)
    {
        if (_chatHub?.State == HubConnectionState.Connected)
            await _chatHub.InvokeAsync("JoinChatRoom", roomId);
    }

    public async Task SendMessage(int roomId, int senderId, string senderType, string content,
        string? fileUrl = null, string? fileName = null, string? fileType = null, long? fileSize = null)
    {
        if (_chatHub?.State == HubConnectionState.Connected)
            await _chatHub.InvokeAsync("SendMessage", roomId, senderId, senderType, content, fileUrl, fileName, fileType, fileSize);
    }

    public async ValueTask DisposeAsync()
    {
        if (_notificationHub != null)
            await _notificationHub.DisposeAsync();
        if (_chatHub != null)
            await _chatHub.DisposeAsync();
    }
}
