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
        Console.WriteLine($"[SignalRService] Initializing with base URL: {baseUrl}");

        try
        {
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

            // Register callback for receiving messages
            _chatConnection.On<object>("ReceiveMessage", (message) =>
            {
                Console.WriteLine($"[SignalRService] ReceiveMessage called");
                OnChatMessageReceived?.Invoke(message);
            });

            // Handle reconnection - re-register callbacks
            _chatConnection.Reconnecting += (error) =>
            {
                Console.WriteLine($"[SignalRService] Chat reconnecting due to error: {error?.Message}");
                return Task.CompletedTask;
            };

            _chatConnection.Reconnected += (connectionId) =>
            {
                Console.WriteLine($"[SignalRService] Chat reconnected with connectionId: {connectionId}");
                return Task.CompletedTask;
            };

            _chatConnection.Closed += (error) =>
            {
                Console.WriteLine($"[SignalRService] Chat connection closed: {error?.Message}");
                return Task.CompletedTask;
            };

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

            // Start connections separately to handle failures
            try
            {
                await _chatConnection.StartAsync();
                Console.WriteLine($"[SignalRService] Chat connection started");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SignalRService] Chat connection failed: {ex.Message}");
            }

            try
            {
                await _notificationConnection.StartAsync();
                Console.WriteLine($"[SignalRService] Notification connection started");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SignalRService] Notification connection failed: {ex.Message}");
            }

            try
            {
                await _installationConnection.StartAsync();
                Console.WriteLine($"[SignalRService] Installation connection started");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SignalRService] Installation connection failed: {ex.Message}");
            }

            Console.WriteLine($"[SignalRService] All connections initialized");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SignalRService] Initialization error: {ex.Message}");
            Console.WriteLine($"[SignalRService] Stack trace: {ex.StackTrace}");
            throw;
        }
    }

    public async Task JoinChatRoom(int chatRoomId)
    {
        Console.WriteLine($"[SignalRService] JoinChatRoom called with chatRoomId: {chatRoomId}");
        Console.WriteLine($"[SignalRService] Chat connection state: {_chatConnection?.State}");
        
        if (_chatConnection != null && _chatConnection.State == HubConnectionState.Connected)
        {
            Console.WriteLine($"[SignalRService] Invoking JoinChatRoom method on server");
            await _chatConnection.InvokeAsync("JoinChatRoom", chatRoomId);
            Console.WriteLine($"[SignalRService] JoinChatRoom invoked successfully");
        }
        else
        {
            Console.WriteLine($"[SignalRService] Cannot join chat room - connection not connected");
        }
    }

    public async Task SendMessage(int chatRoomId, int senderId, string senderType, string content)
    {
        Console.WriteLine($"[SignalRService] SendMessage called - Room:{chatRoomId}, Sender:{senderId}, Type:{senderType}, Content:{content}");
        Console.WriteLine($"[SignalRService] Connection state: {_chatConnection?.State}");
        
        if (_chatConnection != null && _chatConnection.State == HubConnectionState.Connected)
        {
            try
            {
                await _chatConnection.InvokeAsync("SendMessage", chatRoomId, senderId, senderType, content);
                Console.WriteLine($"[SignalRService] SendMessage invoked successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SignalRService] SendMessage failed: {ex.Message}");
                throw;
            }
        }
        else
        {
            Console.WriteLine($"[SignalRService] Cannot send message - connection not connected (State: {_chatConnection?.State})");
            throw new InvalidOperationException("SignalR connection not established");
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
