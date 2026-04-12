namespace Domain.Entities.Shipping;

using Domain.Abstractions;
using Domain.Exceptions;
using Domain.ValueObjects;

/// <summary>
/// ShippingRate entity - represents a shipping rate based on weight range.
/// </summary>
public class ShippingRate : Entity
    {
        public int ZoneId { get; private set; }
        public Weight WeightFrom { get; private set; } = null!;
        public Weight WeightTo { get; private set; } = null!;
        public Money Price { get; private set; } = null!;
        public bool IsActive { get; private set; } = true;

        public virtual ShippingZone Zone { get; private set; } = null!;

        private ShippingRate() { }

        public static ShippingRate Create(int zoneId, Weight from, Weight to, Money price)
        {
            if (zoneId <= 0)
                throw new ValidationException(nameof(zoneId), "ZoneId không hợp lệ");

            if (from.ValueInKg > to.ValueInKg)
                throw new ValidationException(nameof(from), "Cân nặng bắt đầu phải nhỏ hơn kết thúc");

            if (price.IsLessThan(Money.Zero()))
                throw new ValidationException(nameof(price), "Giá ship không thể âm");

            return new ShippingRate
            {
                ZoneId = zoneId,
                WeightFrom = from,
                WeightTo = to,
                Price = price,
                IsActive = true
            };
        }

        // Legacy overload for backward compatibility
        public static ShippingRate Create(int zoneId, decimal from, decimal to, decimal price)
        {
            return Create(zoneId, Weight.FromKilograms(from), Weight.FromKilograms(to), Money.Vnd(price));
        }

        public void UpdatePrice(Money newPrice)
        {
            if (newPrice.IsLessThan(Money.Zero()))
                throw new ValidationException(nameof(newPrice), "Giá không thể âm");

            Price = newPrice;
        }

        public bool Matches(decimal weight) =>
            weight >= WeightFrom.ValueInKg && weight <= WeightTo.ValueInKg;

        public Money GetPrice() => Price;

        public void Activate() => IsActive = true;
        public void Deactivate() => IsActive = false;
    }
