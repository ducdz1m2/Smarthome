using Domain.Services;
using Domain.ValueObjects;

namespace Infrastructure.Services;

public class ShippingService : Domain.Services.IShippingService
{
    // Simple prototype logic - will be replaced with 3rd party integration later
    public Task<Money> CalculateShippingFeeAsync(
        string city,
        string district,
        decimal totalWeight,
        decimal totalAmount,
        bool requiresInstallation = false,
        CancellationToken cancellationToken = default)
    {
        // Installation orders: free shipping
        if (requiresInstallation)
        {
            return Task.FromResult(Money.Vnd(0));
        }

        // Regular orders: 30.000đ for regular orders
        // Free shipping for orders >= 500.000đ
        decimal fee = totalAmount >= 500000 ? 0 : 30000;

        return Task.FromResult(Money.Vnd(fee));
    }

    public Task<IReadOnlyList<ShippingMethodInfo>> GetAvailableShippingMethodsAsync(
        string city,
        string district,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<ShippingMethodInfo>>(new List<ShippingMethodInfo>
        {
            new ShippingMethodInfo
            {
                Code = "STANDARD",
                Name = "Giao hàng tiêu chuẩn",
                Description = "3-5 ngày làm việc",
                Fee = Money.Vnd(30000),
                EstimatedDays = 5,
                IsCodAvailable = true
            }
        });
    }

    public Task<bool> IsDistrictInZoneAsync(
        int zoneId,
        string city,
        string district,
        CancellationToken cancellationToken = default)
    {
        // Prototype - always return true
        return Task.FromResult(true);
    }

    public Task<Domain.Entities.Shipping.ShippingZone?> FindZoneForAddressAsync(
        string city,
        string district,
        CancellationToken cancellationToken = default)
    {
        // Prototype - not using zones for now
        return Task.FromResult<Domain.Entities.Shipping.ShippingZone?>(null);
    }

    public Task<DateTime> GetEstimatedDeliveryDateAsync(
        string city,
        string district,
        DateTime? fromDate = null,
        CancellationToken cancellationToken = default)
    {
        var baseDate = fromDate ?? DateTime.Now;
        // Default 5 business days
        return Task.FromResult(baseDate.AddDays(5));
    }

    public Task<bool> IsShippingAvailableAsync(
        string city,
        string district,
        CancellationToken cancellationToken = default)
    {
        // Prototype - always available
        return Task.FromResult(true);
    }
}
