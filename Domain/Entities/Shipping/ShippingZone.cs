namespace Domain.Entities.Shipping
{
    using Domain.Entities.Common;
    using Domain.Exceptions;

    public class ShippingZone : BaseEntity 
    {
        public string Name { get; private set; } = string.Empty;
        public string? Description { get; private set; }
        public bool IsActive { get; private set; } = true;

        public virtual ICollection<ShippingRate> Rates { get; private set; } = new List<ShippingRate>();

        private ShippingZone() { }

        public static ShippingZone Create(string name, string? description = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new DomainException("Tên vùng giao hàng không được trống");

            return new ShippingZone
            {
                Name = name.Trim(),
                Description = description?.Trim(),
                IsActive = true
            };
        }

        public void Update(string name, string? description)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new DomainException("Tên vùng không được trống");

            Name = name.Trim();
            Description = description?.Trim();
        }

        public void Activate() => IsActive = true;
        public void Deactivate() => IsActive = false;

        public decimal CalculateFee(decimal weight)
        {
            var rate = Rates
                .Where(r => r.IsActive && r.Matches(weight))
                .OrderBy(r => r.Price)
                .FirstOrDefault();

            if (rate == null)
                throw new DomainException($"Không có giá ship cho cân nặng {weight}");

            return rate.Price;
        }
    }
}
