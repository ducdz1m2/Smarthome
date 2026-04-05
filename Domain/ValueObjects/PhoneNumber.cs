using Domain.Exceptions;
using System.Text.RegularExpressions;

namespace Domain.ValueObjects
{
    public sealed record PhoneNumber
    {
        public string Value { get; }
        public string FormattedValue { get; }

        private static readonly Regex PhoneRegex = new(
            @"^(0[3|5|7|8|9])+([0-9]{8})$",
            RegexOptions.Compiled);

        private PhoneNumber(string value, string formatted)
        {
            Value = value;
            FormattedValue = formatted;
        }

        public static PhoneNumber Create(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new DomainException("Số điện thoại không được trống");

            // Remove spaces, dashes, dots
            var cleaned = new string(value.Where(char.IsDigit).ToArray());

            // Validate VN mobile format
            if (!PhoneRegex.IsMatch(cleaned))
                throw new DomainException("Số điện thoại không hợp lệ (phải là số VN di động 10 số)");

            // Format: 0901234567 -> 090 123 4567
            var formatted = $"{cleaned.Substring(0, 4)} {cleaned.Substring(4, 3)} {cleaned.Substring(7)}";

            return new PhoneNumber(cleaned, formatted);
        }

        public bool IsViettel => Value.StartsWith("09") || Value.StartsWith("08");
        public bool IsMobifone => Value.StartsWith("07") || Value.StartsWith("089");
        public bool IsVinaPhone => Value.StartsWith("08") || Value.StartsWith("09");

        public override string ToString() => FormattedValue;

        // EF Core
        private PhoneNumber()
        {
            Value = string.Empty;
            FormattedValue = string.Empty;
        }
    }
}