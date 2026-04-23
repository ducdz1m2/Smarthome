using Domain.Exceptions;

namespace Domain.Entities.Communication;

public class PushSubscription
{
    public int Id { get; private set; }
    public int UserId { get; private set; }
    public string Endpoint { get; private set; } = string.Empty;
    public string P256DH { get; private set; } = string.Empty;
    public string Auth { get; private set; } = string.Empty;
    public string? UserAgent { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? ExpiresAt { get; private set; }

    private PushSubscription() { }

    public static PushSubscription Create(
        int userId,
        string endpoint,
        string p256dh,
        string auth,
        string? userAgent = null,
        DateTime? expiresAt = null)
    {
        if (userId <= 0)
            throw new ValidationException(nameof(userId), "UserId không hợp lệ");

        if (string.IsNullOrWhiteSpace(endpoint))
            throw new ValidationException(nameof(endpoint), "Endpoint không được để trống");

        if (string.IsNullOrWhiteSpace(p256dh))
            throw new ValidationException(nameof(p256dh), "P256DH không được để trống");

        if (string.IsNullOrWhiteSpace(auth))
            throw new ValidationException(nameof(auth), "Auth không được để trống");

        return new PushSubscription
        {
            UserId = userId,
            Endpoint = endpoint,
            P256DH = p256dh,
            Auth = auth,
            UserAgent = userAgent,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = expiresAt
        };
    }

    public void UpdateExpiration(DateTime? expiresAt)
    {
        ExpiresAt = expiresAt;
    }
}
