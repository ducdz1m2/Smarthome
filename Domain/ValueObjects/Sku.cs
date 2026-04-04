using Smarthome.Domain.Exceptions;

namespace Domain.ValueObjects
{
    public sealed record Sku
    {
        public string Value { get; }

        private Sku(string value) => Value = value;

        public static Sku Create(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new DomainException("SKU không được trống");

            if (value.Length < 3 || value.Length > 50)
                throw new DomainException("SKU phải từ 3-50 ký tự");

            // Chỉ chấp nhận chữ, số, gạch ngang, gạch dưới
            var cleaned = value.Trim().ToUpper();
            if (!cleaned.All(c => char.IsLetterOrDigit(c) || c == '-' || c == '_'))
                throw new DomainException("SKU chỉ chứa chữ, số, gạch ngang, gạch dưới");

            return new Sku(cleaned);
        }

        // Generate từ prefix + number
        public static Sku Generate(string prefix, int number)
        {
            if (string.IsNullOrWhiteSpace(prefix))
                throw new DomainException("Prefix không được trống");

            return Create($"{prefix.Trim().ToUpper()}-{number:D6}");
        }

        public override string ToString() => Value;

        // EF Core
        private Sku() => Value = string.Empty;
    }
}