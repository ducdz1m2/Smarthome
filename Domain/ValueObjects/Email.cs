using Smarthome.Domain.Exceptions;
using System.Text.RegularExpressions;

namespace Domain.ValueObjects
{
    public sealed record Email
    {
        public string Value { get; }

        private static readonly Regex EmailRegex = new(
            @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private Email(string value) => Value = value;

        public static Email Create(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new DomainException("Email không được trống");

            var cleaned = value.Trim().ToLower();

            if (!EmailRegex.IsMatch(cleaned))
                throw new DomainException("Email không hợp lệ");

            if (cleaned.Length > 254)
                throw new DomainException("Email quá dài (tối đa 254 ký tự)");

            return new Email(cleaned);
        }

        public string GetDomain() => Value.Split('@')[1];

        public string GetLocalPart() => Value.Split('@')[0];

        public override string ToString() => Value;

        // EF Core
        private Email() => Value = string.Empty;
    }
}