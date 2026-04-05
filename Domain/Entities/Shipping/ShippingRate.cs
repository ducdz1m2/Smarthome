using Domain.Entities.Common;
using Domain.Exceptions;
using Domain.ValueObjects;
using Smarthome.Domain.ValueObjects;

namespace Domain.Entities.Shipping
{


    public class ShippingRate : BaseEntity
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
                throw new DomainException("ZoneId không hợp lệ");

            if (from.Value > to.Value)
                throw new DomainException("Cân nặng bắt đầu phải nhỏ hơn kết thúc");

            if (price.Amount < 0)
                throw new DomainException("Giá ship không thể âm");

            return new ShippingRate
            {
                ZoneId = zoneId,
                WeightFrom = from,
                WeightTo = to,
                Price = price,
                IsActive = true
            };
        }

        public void UpdatePrice(Money newPrice)
        {
            if (newPrice.Amount < 0)
                throw new DomainException("Giá không thể âm");

            Price = newPrice;
        }

        public bool Matches(Weight weight) =>
            weight.Value >= WeightFrom.Value && weight.Value <= WeightTo.Value;

        public void Activate() => IsActive = true;
        public void Deactivate() => IsActive = false;
    }
}
