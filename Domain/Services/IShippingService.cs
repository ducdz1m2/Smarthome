using Domain.Entities.Shipping;
using Domain.ValueObjects;

namespace Domain.Services;

/// <summary>
/// Domain service for shipping calculations and zone management.
/// </summary>
public interface IShippingService
{
    /// <summary>
    /// Calculate shipping fee for a destination.
    /// </summary>
    Task<Money> CalculateShippingFeeAsync(
        string city,
        string district,
        decimal totalWeight,
        decimal totalAmount,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get available shipping methods for a destination.
    /// </summary>
    Task<IReadOnlyList<ShippingMethodInfo>> GetAvailableShippingMethodsAsync(
        string city,
        string district,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if a district is within a shipping zone.
    /// </summary>
    Task<bool> IsDistrictInZoneAsync(
        int zoneId,
        string city,
        string district,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Find the shipping zone for an address.
    /// </summary>
    Task<ShippingZone?> FindZoneForAddressAsync(
        string city,
        string district,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get estimated delivery date.
    /// </summary>
    Task<DateTime> GetEstimatedDeliveryDateAsync(
        string city,
        string district,
        DateTime? fromDate = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validate if shipping is available to an address.
    /// </summary>
    Task<bool> IsShippingAvailableAsync(
        string city,
        string district,
        CancellationToken cancellationToken = default);
}

public record ShippingMethodInfo
{
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public Money Fee { get; init; } = Money.Zero();
    public int EstimatedDays { get; init; }
    public bool IsCodAvailable { get; init; }
}
