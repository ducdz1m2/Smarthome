using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.Components;

namespace Web.Services;

public class SignalRService
{
    private HubConnection? _chatConnection;
    private HubConnection? _notificationConnection;
    private HubConnection? _installationConnection;
    private readonly NavigationManager _navigationManager;
    private readonly HashSet<int> _pendingRoomJoins = new();
    private readonly object _lock = new();

    public event Action<object>? OnChatMessageReceived;
    public event Action<object>? OnNotificationReceived;
    public event Action<string>? OnInstallationUpdate;

    public SignalRService(NavigationManager navigationManager)
    {
        _navigationManager = navigationManager;
    }

    private bool _isInitialized = false;

    public async Task InitializeAsync()
    {
        if (_isInitialized)
        {
            Console.WriteLine($"[SignalRService] Already initialized, skipping");
            return;
        }

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

            _chatConnection.Reconnected += async (connectionId) =>
            {
                Console.WriteLine($"[SignalRService] Chat reconnected with connectionId: {connectionId}");
                // Re-join all pending rooms
                lock (_lock)
                {
                    foreach (var roomId in _pendingRoomJoins.ToList())
                    {
                        Console.WriteLine($"[SignalRService] Re-joining room {roomId} after reconnection");
                        _ = JoinChatRoomInternalAsync(roomId);
                    }
                }
                await Task.CompletedTask;
            };

            _chatConnection.Closed += (error) =>
            {
                Console.WriteLine($"[SignalRService] Chat connection closed: {error?.Message}");
                return Task.CompletedTask;
            };

            _notificationConnection.On<object>("ReceiveNotification", (notification) =>
            {
                Console.WriteLine($"[SignalRService] ReceiveNotification called with: {notification}");
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
                // Process any pending room joins
                lock (_lock)
                {
                    foreach (var roomId in _pendingRoomJoins.ToList())
                    {
                        Console.WriteLine($"[SignalRService] Processing pending join for room {roomId}");
                        _ = JoinChatRoomInternalAsync(roomId);
                    }
                }
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

            _isInitialized = true;
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
        
        // Always add to pending set
        lock (_lock)
        {
            _pendingRoomJoins.Add(chatRoomId);
        }
        
        // Try to join immediately if connected
        await JoinChatRoomInternalAsync(chatRoomId);
    }
    
    private async Task JoinChatRoomInternalAsync(int chatRoomId)
    {
        Console.WriteLine($"[SignalRService] JoinChatRoomInternalAsync called with chatRoomId: {chatRoomId}");
        Console.WriteLine($"[SignalRService] Chat connection state: {_chatConnection?.State}");
        
        if (_chatConnection != null && _chatConnection.State == HubConnectionState.Connected)
        {
            try
            {
                Console.WriteLine($"[SignalRService] Invoking JoinChatRoom method on server");
                await _chatConnection.InvokeAsync("JoinChatRoom", chatRoomId);
                Console.WriteLine($"[SignalRService] JoinChatRoom invoked successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SignalRService] Failed to join room {chatRoomId}: {ex.Message}");
            }
        }
        else
        {
            Console.WriteLine($"[SignalRService] Cannot join chat room {chatRoomId} - connection not connected (will retry when connected)");
        }
    }

    public async Task SendMessage(int chatRoomId, int senderId, string senderType, string content, string? fileUrl = null, string? fileName = null, string? fileType = null, long? fileSize = null)
    {
        Console.WriteLine($"[SignalRService] SendMessage called - Room:{chatRoomId}, Sender:{senderId}, Type:{senderType}, Content:{content}, FileUrl:{fileUrl}");
        Console.WriteLine($"[SignalRService] Connection state: {_chatConnection?.State}");
        
        if (_chatConnection != null && _chatConnection.State == HubConnectionState.Connected)
        {
            try
            {
                await _chatConnection.InvokeAsync("SendMessage", chatRoomId, senderId, senderType, content, fileUrl, fileName, fileType, fileSize);
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
