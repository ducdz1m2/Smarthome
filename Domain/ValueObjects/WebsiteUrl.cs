
using Domain.Exceptions;

namespace Domain.ValueObjects
{
    public sealed record WebsiteUrl
    {
        public string Value { get; }

        private WebsiteUrl(string value) => Value = value;

        public static WebsiteUrl? Create(string? url)
        {
            if (string.IsNullOrWhiteSpace(url)) return null;

            var trimmed = url.Trim();

            if (!trimmed.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                !trimmed.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                trimmed = "https://" + trimmed;
            }

            if (!Uri.TryCreate(trimmed, UriKind.Absolute, out var uri))
                throw new DomainException($"Website URL không hợp lệ: '{url}'");

            if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
                throw new DomainException("Website phải dùng http hoặc https");

            return new WebsiteUrl(trimmed.ToLower());
        }

        public override string ToString() => Value;

        // EF Core
        private WebsiteUrl() => Value = string.Empty;
    }
}
