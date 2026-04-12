namespace Domain.Entities.Shipping;

using Domain.Abstractions;
using Domain.Exceptions;
using Domain.ValueObjects;

/// <summary>
/// ShippingZone aggregate root - represents a shipping zone with rates.
/// </summary>
public class ShippingZone : AggregateRoot 
    {
        public string Name { get; private set; } = string.Empty;
        public string? Description { get; private set; }
        public bool IsActive { get; private set; } = true;

        public virtual ICollection<ShippingRate> Rates { get; private set; } = new List<ShippingRate>();

        private ShippingZone() { }

        public static ShippingZone Create(string name, string? description = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ValidationException(nameof(name), "Tên vùng giao hàng không được trống");

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
                throw new ValidationException(nameof(name), "Tên vùng không được trống");

            Name = name.Trim();
            Description = description?.Trim();
        }

        public void Activate() => IsActive = true;
        public void Deactivate() => IsActive = false;

        public Money CalculateFee(Weight weight)
        {
            var rate = Rates
                .Where(r => r.IsActive && r.Matches(weight.ValueInKg))
                .OrderBy(r => r.Price.Amount)
                .FirstOrDefault();

            if (rate == null)
                throw new BusinessRuleViolationException("NoShippingRate", $"Không có giá ship cho cân nặng {weight.ValueInKg}kg");

            return rate.GetPrice();
        }

        // Legacy overload for backward compatibility
        public decimal CalculateFee(decimal weight)
        {
            return CalculateFee(Weight.FromKilograms(weight)).Amount;
        }
    }
