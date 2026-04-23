using Application.Interfaces.Services;
using Domain.Entities.Communication;
using Domain.Repositories;
using Microsoft.Extensions.Configuration;
using WebPush;
using System.Text.Json;

namespace Application.Services;

public class PushNotificationService : IPushNotificationService
{
    private readonly IPushSubscriptionRepository _subscriptionRepository;
    private readonly IConfiguration _configuration;

    public PushNotificationService(
        IPushSubscriptionRepository subscriptionRepository,
        IConfiguration configuration)
    {
        _subscriptionRepository = subscriptionRepository;
        _configuration = configuration;
    }

    public async Task SendNotificationAsync(int userId, string title, string message, string? actionUrl = null)
    {
        var subscriptions = await _subscriptionRepository.GetByUserIdAsync(userId);
        
        if (subscriptions.Count == 0)
        {
            Console.WriteLine($"[PushNotificationService] No subscriptions found for user {userId}");
            return;
        }

        var vapidDetails = new VapidDetails(
            _configuration["VapidSettings:Subject"],
            _configuration["VapidSettings:PublicKey"],
            _configuration["VapidSettings:PrivateKey"]
        );

        var payload = new
        {
            title = title,
            body = message,
            icon = "/icon-192.png",
            badge = "/badge-72.png",
            actionUrl = actionUrl
        };

        var payloadJson = JsonSerializer.Serialize(payload);

        foreach (var subscription in subscriptions)
        {
            try
            {
                var webPushSubscription = new WebPush.PushSubscription(
                    subscription.Endpoint,
                    subscription.P256DH,
                    subscription.Auth
                );

                var webPushClient = new WebPushClient();
                await webPushClient.SendNotificationAsync(webPushSubscription, payloadJson, vapidDetails);
                
                Console.WriteLine($"[PushNotificationService] Sent notification to user {userId} via endpoint {subscription.Endpoint}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PushNotificationService] Failed to send notification to user {userId}: {ex.Message}");
                
                // Remove invalid subscription
                if (ex.Message.Contains("410 Gone") || ex.Message.Contains("404 Not Found"))
                {
                    _subscriptionRepository.Delete(subscription);
                    await _subscriptionRepository.SaveChangesAsync();
                }
            }
        }
    }

    public async Task SendNotificationToMultipleAsync(List<int> userIds, string title, string message, string? actionUrl = null)
    {
        foreach (var userId in userIds)
        {
            await SendNotificationAsync(userId, title, message, actionUrl);
        }
    }
}
