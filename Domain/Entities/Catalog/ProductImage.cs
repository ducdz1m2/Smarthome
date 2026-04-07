

using Domain.Entities.Common;
using Domain.Exceptions;

namespace Domain.Entities.Catalog
{
    public class ProductImage : BaseEntity
    {
        public int ProductId { get; private set; }
        public string Url { get; private set; } = string.Empty;
        public string? AltText { get; private set; }
        public bool IsMain { get; private set; } = false;
        public int SortOrder { get; private set; } = 0;

        // Navigation
        public virtual Product Product { get; private set; } = null!;

        private ProductImage() { } // EF Core

        public static ProductImage Create(int productId, string url, string? altText = null, bool isMain = false, int sortOrder = 0)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new DomainException("URL hình ảnh không được trống");

            if (!Uri.TryCreate(url, UriKind.Absolute, out _))
                throw new DomainException("URL hình ảnh không hợp lệ");

            return new ProductImage
            {
                ProductId = productId,
                Url = url.Trim(),
                AltText = altText?.Trim(),
                IsMain = isMain,
                SortOrder = sortOrder
            };
        }

        public void SetAsMain()
        {
            IsMain = true;
        }

        public void SetAsSecondary()
        {
            IsMain = false;
        }

        public void UpdateSortOrder(int newOrder)
        {
            SortOrder = newOrder;
        }

        public void UpdateAltText(string? altText)
        {
            AltText = altText?.Trim();
        }
    }
}
