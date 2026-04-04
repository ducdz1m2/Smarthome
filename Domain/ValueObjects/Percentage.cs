using Smarthome.Domain.Exceptions;

namespace Domain.ValueObjects
{
    public sealed record Percentage
    {
        public decimal Value { get; }

        private Percentage(decimal value)
        {
            if (value < 0 || value > 100)
                throw new DomainException("Phần trăm phải từ 0 đến 100");

            Value = value;
        }

        public static Percentage Create(decimal value) => new(value);

        public static Percentage FromFraction(decimal fraction) =>
            new(fraction * 100);

        public static Percentage Zero => new(0);
        public static Percentage Hundred => new(100);

        public decimal ToFraction() => Value / 100;

        public Money ApplyTo(Money amount) => amount.ApplyDiscount(this);

        public Percentage Add(Percentage other) =>
            new(Math.Min(100, Value + other.Value));

        public bool IsGreaterThan(Percentage other) => Value > other.Value;

        public override string ToString() => $"{Value}%";

        // EF Core
        private Percentage() => Value = 0;
    }
}