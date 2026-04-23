using Application.Interfaces.Services;
using Domain.Entities.Communication;
using Domain.Interfaces;
using Domain.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json.Serialization;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[IgnoreAntiforgeryToken]
public class PushSubscriptionController : ControllerBase
{
    private readonly IPushSubscriptionRepository _subscriptionRepository;
    private readonly ICurrentUserService _currentUserService;

    public PushSubscriptionController(
        IPushSubscriptionRepository subscriptionRepository,
        ICurrentUserService currentUserService)
    {
        _subscriptionRepository = subscriptionRepository;
        _currentUserService = currentUserService;
    }

    [HttpPost("subscribe")]
    public async Task<IActionResult> Subscribe([FromBody] SubscribeRequest request)
    {
        try
        {
            Console.WriteLine($"[PushSubscriptionController] Subscribe request received");
            Console.WriteLine($"[PushSubscriptionController] Request body: {System.Text.Json.JsonSerializer.Serialize(request)}");
            Console.WriteLine($"[PushSubscriptionController] Endpoint: {request.Endpoint}");
            Console.WriteLine($"[PushSubscriptionController] P256DH: {request.Keys?.P256DH?.Substring(0, Math.Min(20, request.Keys?.P256DH?.Length ?? 0))}...");
            Console.WriteLine($"[PushSubscriptionController] Auth: {request.Keys?.Auth?.Substring(0, Math.Min(20, request.Keys?.Auth?.Length ?? 0))}...");

            // Get UserId from ClaimsPrincipal
            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                // Try getting from NameIdentifier
                userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            }

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                Console.WriteLine($"[PushSubscriptionController] UserId is null or invalid");
                Console.WriteLine($"[PushSubscriptionController] User.Identity?.IsAuthenticated: {User.Identity?.IsAuthenticated}");
                Console.WriteLine($"[PushSubscriptionController] User.Identity?.Name: {User.Identity?.Name}");
                return Unauthorized(new { message = "User not authenticated" });
            }

            Console.WriteLine($"[PushSubscriptionController] UserId: {userId}");

            // Check if subscription already exists
            var existing = await _subscriptionRepository.GetByEndpointAsync(request.Endpoint);
            if (existing != null)
            {
                // Update existing subscription
                existing.UpdateExpiration(request.ExpiresAt);
                _subscriptionRepository.Update(existing);
                await _subscriptionRepository.SaveChangesAsync();
                return Ok(new { message = "Subscription updated" });
            }

            // Create new subscription
            var subscription = PushSubscription.Create(
                userId,
                request.Endpoint,
                request.Keys.P256DH,
                request.Keys.Auth,
                Request.Headers["User-Agent"].ToString(),
                request.ExpiresAt
            );

            await _subscriptionRepository.AddAsync(subscription);
            await _subscriptionRepository.SaveChangesAsync();

            Console.WriteLine($"[PushSubscriptionController] Subscription saved successfully");
            return Ok(new { message = "Subscribed successfully" });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[PushSubscriptionController] Error: {ex.Message}");
            Console.WriteLine($"[PushSubscriptionController] StackTrace: {ex.StackTrace}");
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("unsubscribe")]
    public async Task<IActionResult> Unsubscribe([FromBody] UnsubscribeRequest request)
    {
        // Get UserId from ClaimsPrincipal
        var userIdClaim = User.FindFirst("UserId")?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
        {
            userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        }

        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var subscription = await _subscriptionRepository.GetByEndpointAsync(request.Endpoint);
        if (subscription == null)
        {
            return NotFound(new { message = "Subscription not found" });
        }

        if (subscription.UserId != userId)
        {
            return Forbid();
        }

        _subscriptionRepository.Delete(subscription);
        await _subscriptionRepository.SaveChangesAsync();

        return Ok(new { message = "Unsubscribed successfully" });
    }

    [HttpGet("vapid-public-key")]
    [AllowAnonymous]
    public IActionResult GetVapidPublicKey()
    {
        var publicKey = HttpContext.RequestServices.GetRequiredService<Microsoft.Extensions.Configuration.IConfiguration>()
            ["VapidSettings:PublicKey"];
        
        return Ok(new { publicKey });
    }
}

public class SubscribeRequest
{
    [JsonPropertyName("endpoint")]
    public string Endpoint { get; set; } = string.Empty;
    [JsonPropertyName("keys")]
    public PushKeys Keys { get; set; } = new();
    [JsonPropertyName("expiresAt")]
    public DateTime? ExpiresAt { get; set; }
}

public class PushKeys
{
    [JsonPropertyName("p256dh")]
    public string P256DH { get; set; } = string.Empty;
    [JsonPropertyName("auth")]
    public string Auth { get; set; } = string.Empty;
}

public class UnsubscribeRequest
{
    [JsonPropertyName("endpoint")]
    public string Endpoint { get; set; } = string.Empty;
}
