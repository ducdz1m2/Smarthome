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

        // Regular orders: tiered pricing based on order amount
        // Orders < 5,000,000đ: 100,000đ
        // Orders 5,000,000đ - 15,000,000đ: 50,000đ
        // Orders 15,000,000đ - 25,000,000đ: 30,000đ
        // Orders >= 25,000,000đ: Free shipping
        decimal fee = totalAmount switch
        {
            >= 25000000 => 0,
            >= 15000000 => 30000,
            >= 5000000 => 50000,
            _ => 100000
        };

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
                Description = "3-5 ngày làm việc. Phí: 30k-100k, miễn phí đơn >= 25 triệu",
                Fee = Money.Vnd(50000),
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
