using Domain.Exceptions;

namespace Domain.ValueObjects;

public sealed record Weight
{
    public decimal Value { get; }
    public decimal ValueInKg => Value;
    public string Unit { get; } = "kg";

    private Weight(decimal value)
    {
        if (value < 0)
            throw new DomainException("Cân nặng không thể âm");

        Value = value;
    }

    public static Weight Create(decimal kg) => new(kg);
    public static Weight FromKilograms(decimal kg) => new(kg);
    public static Weight FromGrams(int grams) => new(grams / 1000m);

    public bool IsHeavierThan(Weight other) => Value > other.Value;
    public Weight Add(Weight other) => new(Value + other.Value);

    public override string ToString() => $"{Value:F2} {Unit}";

    private Weight() => Value = 0;
}
