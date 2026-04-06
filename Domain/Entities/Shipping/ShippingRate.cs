using Domain.Entities.Common;
using Domain.Exceptions;

namespace Domain.Entities.Shipping
{
    public class ShippingRate : BaseEntity
    {
        public int ZoneId { get; private set; }
        public decimal WeightFrom { get; private set; }
        public decimal WeightTo { get; private set; }
        public decimal Price { get; private set; }
        public bool IsActive { get; private set; } = true;

        public virtual ShippingZone Zone { get; private set; } = null!;

        private ShippingRate() { }

        public static ShippingRate Create(int zoneId, decimal from, decimal to, decimal price)
        {
            if (zoneId <= 0)
                throw new DomainException("ZoneId không hợp lệ");

            if (from > to)
                throw new DomainException("Cân nặng bắt đầu phải nhỏ hơn kết thúc");

            if (price < 0)
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

        public void UpdatePrice(decimal newPrice)
        {
            if (newPrice < 0)
                throw new DomainException("Giá không thể âm");

            Price = newPrice;
        }

        public bool Matches(decimal weight) =>
            weight >= WeightFrom && weight <= WeightTo;

        public void Activate() => IsActive = true;
        public void Deactivate() => IsActive = false;
    }
}
