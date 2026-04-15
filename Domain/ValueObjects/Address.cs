using Domain.Exceptions;

namespace Domain.ValueObjects
{
    public sealed record Address
    {
        public string Street { get; }
        public string? Ward { get; }
        public string District { get; }
        public string City { get; }
        public string? Country { get; }
        public string? PostalCode { get; }

        private Address(string street, string? ward, string district, string city,
            string? country = "Việt Nam", string? postalCode = null)
        {
            if (string.IsNullOrWhiteSpace(street))
                throw new DomainException("Địa chỉ (số nhà, đường) không được trống");

            if (string.IsNullOrWhiteSpace(district))
                throw new DomainException("Quận/Huyện không được trống");

            if (string.IsNullOrWhiteSpace(city))
                throw new DomainException("Tỉnh/Thành phố không được trống");

            Street = street.Trim();
            Ward = ward?.Trim();
            District = district.Trim();
            City = city.Trim();
            Country = country?.Trim() ?? "Việt Nam";
            PostalCode = postalCode?.Trim();
        }

        public static Address Create(string street, string? ward, string district,
            string city, string? country = null, string? postalCode = null) =>
            new(street, ward, district, city, country, postalCode);

        // Quick create for VN address
        public static Address Vn(string street, string ward, string district, string city) =>
            new(street, ward, district, city, "Việt Nam", null);

        public string ToFullString()
        {
            var parts = new List<string> { Street };

            if (!string.IsNullOrWhiteSpace(Ward) && Ward != "N/A")
                parts.Add(Ward);

            if (!string.IsNullOrWhiteSpace(District) && District != "N/A")
                parts.Add(District);

            if (!string.IsNullOrWhiteSpace(City) && City != "N/A")
                parts.Add(City);

            if (!string.IsNullOrWhiteSpace(Country) && Country != "Việt Nam")
                parts.Add(Country);

            return string.Join(", ", parts);
        }

        public string ToShortString()
        {
            var parts = new List<string> { Street };

            if (!string.IsNullOrWhiteSpace(District) && District != "N/A")
                parts.Add(District);

            if (!string.IsNullOrWhiteSpace(City) && City != "N/A")
                parts.Add(City);

            return string.Join(", ", parts);
        }

        public override string ToString() => ToFullString();

        // EF Core
        private Address()
        {
            Street = string.Empty;
            District = string.Empty;
            City = string.Empty;
        }
    }
}