namespace Application.Interfaces.Services;

public interface IPushNotificationService
{
    Task SendNotificationAsync(int userId, string title, string message, string? actionUrl = null);
    Task SendNotificationToMultipleAsync(List<int> userIds, string title, string message, string? actionUrl = null);
}
